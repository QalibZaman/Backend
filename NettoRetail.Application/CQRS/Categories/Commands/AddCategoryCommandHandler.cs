using MapsterMapper;
using MediatR;
using NettoRetail.Application.Exceptions;
using NettoRetail.Application.Interfaces;
using NettoRetail.Application.Models.Request.Category;
using NettoRetail.Application.Models.Response.Category;
using NettoRetail.DataAccess.Specifications.Categories;
using NettoRetail.DataAccess.Specifications.Shop;
using NettoRetail.Domain.Entities;
using NettoRetail.Shared.Interfaces.Repositories;
using System.Text;

namespace NettoRetail.Application.CQRS.Categories.Commands;
public record AddCategoryCommand(AddCategoryRequest Model, int ShopId) : IRequest<CategoryResponse>;

public class AddCategoryCommandHandler(
    ICategoryRepository categoryRepository,
    ICategoryLevelManagementService levelService,
    IShopRepository shopRepository,
    IMapper mapper) : IRequestHandler<AddCategoryCommand, CategoryResponse>
{
    public async Task<CategoryResponse> Handle(AddCategoryCommand request, CancellationToken cancellationToken)
    {
        var normalizedName = request.Model.Name.Trim();

        var normalizedUrl = await CreateCustomUrlAsync(request.Model.ParentId, request.ShopId, request.Model.Name, cancellationToken);

        await ValidateCategoryUniquenessAsync(normalizedName, normalizedUrl, request.ShopId, cancellationToken);

        await ValidateShopExistsAsync(request.ShopId, cancellationToken);

        var category = mapper.Map<CategoryEntity>(request.Model);
        category.Name = normalizedName;
        category.URL = normalizedUrl;
        category.ShopId = request.ShopId;

        await levelService.SetUpCategoryLevel(category, cancellationToken);

        var responseModel = await categoryRepository.AddAsync(category, cancellationToken);
        var response = mapper.Map<CategoryResponse>(responseModel);

        return response;
    }

    private async Task ValidateCategoryUniquenessAsync(string name, string url, int shopId, CancellationToken cancellationToken)
    {
        var specification = new GetCategoriesByNameOrUrlSpecification(name, url, shopId);
        var conflictExists = await categoryRepository.Exists(specification, cancellationToken);

        if (conflictExists)
            throw new ConflictException("Category with such name already exists");
    }

    private async Task<string> CreateCustomUrlAsync(int? parentId, int shopId, string categoryName,  CancellationToken cancellationToken)
    {
        StringBuilder stringBuilder = new StringBuilder();
        if (parentId != null)
        {
            var parent = await categoryRepository.GetAsync(
                new GetCategoryByIdAndShopIdSpecification(parentId.Value, shopId),
                cancellationToken);

            if (parent == null)
                throw new NotFoundException("Parent category not found");

            stringBuilder.Append($"{parent.URL.ToLower().Trim()}/");
        }

        stringBuilder.Append(categoryName.ToLower().Trim());
        if (stringBuilder.Length > 255)
        {
            throw new BadRequestException("Generated URL is too long. Please choose a shorter name or a different parent category.");
        }

        return stringBuilder.ToString();
    }

    private async Task ValidateShopExistsAsync(int shopId, CancellationToken cancellationToken)
    {
        var shopExists = await shopRepository.Exists(
            new GetShopByIdSpecification(shopId),
            cancellationToken);

        if (!shopExists)
            throw new NotFoundException($"No shop with id: {shopId}");
    }
}
