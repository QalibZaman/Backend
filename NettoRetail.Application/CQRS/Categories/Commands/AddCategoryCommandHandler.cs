using Mapster;
using MediatR;
using NettoRetail.Application.Exceptions;
using NettoRetail.Application.Interfaces;
using NettoRetail.Application.Models.Response;
using NettoRetail.DataAccess.Specifications.Categories;
using NettoRetail.DataAccess.Specifications.Shop;
using NettoRetail.Domain.Entities;
using NettoRetail.Shared.Interfaces.Repositories;

namespace NettoRetail.Application.CQRS.Categories.Commands;
public record AddCategoryCommand(string Name, string URL, bool Visibility, int? ParentId, int ShopId) : IRequest<CategoryResponse>;

public class AddCategoryCommandHandler(ICategoryRepository categoryRepository, ICategoryLevelManagementService levelService, IShopRepository shopRepository) : IRequestHandler<AddCategoryCommand, CategoryResponse>
{
    public async Task<CategoryResponse> Handle(AddCategoryCommand request, CancellationToken cancellationToken)
    {
        var getCategoriesByNameOrUrlSpecification = new GetCategoriesByNameOrUrlSpecification(request.Name, request.URL, request.ShopId);

        var conflictModel = await categoryRepository.GetAsync(getCategoriesByNameOrUrlSpecification, cancellationToken);

        if (conflictModel != null)
        {
            throw new ConflictException("Category with such name or url is already exist");
        }

        var shopSpecification = new GetShopByIdSpecification(request.ShopId);

        var shop = await shopRepository.GetAsync(shopSpecification, cancellationToken);

        if (shop == null)
        {
            throw new NotFoundException($"No shop with id: {request.ShopId}");
        }

        var category = request.Adapt<CategoryEntity>();

        await levelService.SetUpCategoryLevel(category, cancellationToken);

        var responseModel = await categoryRepository.AddAsync(category, cancellationToken);

        var response = responseModel.Adapt<CategoryResponse>();

        return response;
    }
}
