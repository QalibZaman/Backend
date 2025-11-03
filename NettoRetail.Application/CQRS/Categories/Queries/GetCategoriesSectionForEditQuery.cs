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

        var specification = new GetCategoriesByIdsSpecification(request.ShopId);
        specification.AddOrderBy(c => c.Name);

        var categories = await categoryRepository.GetAllNestedAsync(specification, cancellationToken);

        if (categories.Count == 0)
        {
            throw new NotFoundException(
                "Your shop does not have any categories yet. Please, add categories to continue");
        }

        const int maxVisibleCategories = 6;

        var dto = new EditCategoriesSectionRequest
        {
            ShopId = shop.Id,
            Visibility = shop.CategoriesSectionVisibility,
            Preview = shop.CategoriesSectionPreview ?? string.Empty,
            Name = shop.CategoriesSectionName ?? string.Empty,
            NumberOfCategories = categories.Count >= maxVisibleCategories ? maxVisibleCategories : categories.Count
        };

        return dto;
    }
}