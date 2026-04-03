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
        var (normalizedName, normalizedUrl) = NormalizeCategoryInput(request.Model);

        await ValidateCategoryUniquenessAsync(normalizedName, normalizedUrl, request.ShopId, cancellationToken);

        if (request.Model.ParentId is int parentId)
        {
            await ValidateParentCategoryExistsAsync(parentId, request.ShopId, cancellationToken);
        }

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

    private static (string Name, string Url) NormalizeCategoryInput(AddCategoryRequest model)
    {
        return (model.Name.Trim(), model.URL.Trim());
    }

    private async Task ValidateCategoryUniquenessAsync(string name, string url, int shopId, CancellationToken cancellationToken)
    {
        var specification = new GetCategoriesByNameOrUrlSpecification(name, url, shopId);
        var conflictExists = await categoryRepository.Exists(specification, cancellationToken);

        if (conflictExists)
            throw new ConflictException("Category with such name or url already exists");
    }

    private async Task ValidateParentCategoryExistsAsync(int parentId, int shopId, CancellationToken cancellationToken)
    {
        var parentExists = await categoryRepository.Exists(
            new GetCategoryByIdAndShopIdSpecification(parentId, shopId),
            cancellationToken);

        if (!parentExists)
            throw new NotFoundException("Parent category not found");
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
