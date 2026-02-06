using Mapster;
using MediatR;
using NettoRetail.Application.Exceptions;
using NettoRetail.Application.Models.Request.Category;
using NettoRetail.Application.Models.Response;
using NettoRetail.DataAccess.Specifications.Categories;
using NettoRetail.Shared.Interfaces.Repositories;

namespace NettoRetail.Application.CQRS.Categories.Queries;

public record GetCategoriesSectionQuery(int ShopId) : IRequest<GetCategoriesSectionResponse>;

public class GetCategoriesSectionForEditQueryHandler(
    ICategoryRepository categoryRepository,
    IShopRepository shopRepository)
    : IRequestHandler<GetCategoriesSectionQuery, GetCategoriesSectionResponse>
{
    public async Task<GetCategoriesSectionResponse> Handle(GetCategoriesSectionQuery request, CancellationToken cancellationToken)
    {
        var shop = await shopRepository.GetByIdAsync(request.ShopId, cancellationToken);
        if (shop is null)
        {
            throw new NotFoundException($"Shop with id {request.ShopId} not found");
        }

        var specification = new GetCategoriesByIdsSpecification(request.ShopId);
        specification.AddOrderBy(c => c.Name);

        var countCategory = await categoryRepository.GetAmountBySpecAsync(specification, cancellationToken);

        if (countCategory == 0)
        {
            throw new NotFoundException(
                "Your shop does not have any categories yet. Please, add categories to continue");
        }

        const int MaxVisibleCategories = 6;

        var dto = new GetCategoriesSectionResponse
        {
            ShopId = shop.Id,
            Visibility = shop.CategoriesSectionVisibility,
            Preview = shop.CategoriesSectionPreview ?? string.Empty,
            Name = shop.CategoriesSectionName ?? string.Empty,
            NumberOfCategories = Math.Min(countCategory, MaxVisibleCategories)
        };

        return dto;
    }
}