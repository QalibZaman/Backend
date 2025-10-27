using Mapster;
using MediatR;
using NettoRetail.Application.Exceptions;
using NettoRetail.Application.Models.Request.Category;
using NettoRetail.DataAccess.Specifications.Categories;
using NettoRetail.Shared.Interfaces.Repositories;

namespace NettoRetail.Application.CQRS.Categories.Queries;

public record GetCategoriesSectionForEditQuery(int ShopId) : IRequest<EditCategoriesSectionRequest>;

public class GetCategoriesSectionForEditQueryHandler(
    ICategoryRepository categoryRepository,
    IShopRepository shopRepository)
    : IRequestHandler<GetCategoriesSectionForEditQuery, EditCategoriesSectionRequest>
{
    public async Task<EditCategoriesSectionRequest> Handle(GetCategoriesSectionForEditQuery request, CancellationToken cancellationToken)
    {
        var shop = await shopRepository.GetByIdAsync(request.ShopId, cancellationToken);
        if (shop is null)
        {
            throw new NotFoundException($"Shop with id {request.ShopId} not found");
        }

        var categories = await categoryRepository.GetAllNestedAsync(new GetCategoriesByIdsSpecification(request.ShopId),
            cancellationToken);

        if (categories.Count == 0)
        {
            throw new NotFoundException(
                "Your shop does not have any categories yet. Please, add categories to continue");
        }

        var orderedCategories = categories.OrderBy(c => c.Name).ToList();
        const int maxVisibleCategories = 6;

        var dto = new EditCategoriesSectionRequest
        {
            ShopId = shop.Id,
            Visibility = shop.CategoriesSectionVisibility,
            Preview = shop.CategoriesSectionPreview ?? string.Empty,
            Name = shop.CategoriesSectionName ?? string.Empty,  
            NumberOfCategories = orderedCategories.Count >= maxVisibleCategories ? maxVisibleCategories : orderedCategories.Count
        };

        return dto;
    }
}