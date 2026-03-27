using MapsterMapper;
using MediatR;
using NettoRetail.Application.Exceptions;
using NettoRetail.Application.Interfaces;
using NettoRetail.Application.Models.Request.Category;
using NettoRetail.Application.Models.Response.Category;
using NettoRetail.Application.Services.ICategoryService;
using NettoRetail.DataAccess.Specifications.Categories;
using NettoRetail.DataAccess.Specifications.Shop;
using NettoRetail.Domain.Entities;
using NettoRetail.Shared.Interfaces.Repositories;

namespace NettoRetail.Application.CQRS.Categories.Commands;

public record AddCategoryCommand(AddCategoryRequest Model, int ShopId) : IRequest<CategoryResponse>;

public class AddCategoryCommandHandler(
    ICategoryRepository categoryRepository,
    ICategoryLevelManagementService levelService,
    ICategoryService categoryService,
    IShopRepository shopRepository,
    IMapper mapper) : IRequestHandler<AddCategoryCommand, CategoryResponse>
{
    public async Task<CategoryResponse> Handle(AddCategoryCommand request, CancellationToken cancellationToken)
    {
        var parentCategory = default(CategoryEntity);
        if (request.Model.ParentId is int parentId)
        {
            var parentCategoryExists = await categoryRepository.Exists(
                new GetCategoryByIdAndShopIdSpecification(parentId, request.ShopId),
                cancellationToken);
            parentCategory = await categoryRepository.GetAsync(
                new GetCategoryByIdAndShopIdSpecification(parentId, request.ShopId),
                cancellationToken);
            if (!parentCategoryExists)
            {
                throw new NotFoundException("Parent category not found");
            }
        }

        var normalizedUrl = categoryService.BuildCategoryUrl(request.Model.Name, null, parentCategory?.URL);
        var getCategoriesByNameOrUrlSpecification = new GetCategoriesByNameOrUrlSpecification(request.Model.Name, normalizedUrl, request.ShopId);

        var conflictingCategoryExists = await categoryRepository.Exists(getCategoriesByNameOrUrlSpecification, cancellationToken);

        if (conflictingCategoryExists)
        {
            throw new ConflictException("Category with such name or url already exists");
        }

        var shopSpecification = new GetShopByIdSpecification(request.ShopId);
        var shopExists = await shopRepository.Exists(shopSpecification, cancellationToken);

        if (!shopExists)
        {
            throw new NotFoundException($"No shop with id: {request.ShopId}");
        }

        var category = mapper.Map<CategoryEntity>(request.Model);
        category.Name = request.Model.Name;
        category.URL = normalizedUrl;
        category.ShopId = request.ShopId;
        await levelService.SetUpCategoryLevel(category, cancellationToken);

        var responseModel = await categoryRepository.AddAsync(category, cancellationToken);

        var response = mapper.Map<CategoryResponse>(responseModel);

        return response;
    }
}
