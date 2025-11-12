using MediatR;
using NettoRetail.Application.Interfaces;
using NettoRetail.Application.Models.Request;
using NettoRetail.Application.Models.Request.Offer;
using NettoRetail.Application.Models.Response;
using NettoRetail.Application.Services.IBrandService;
using NettoRetail.Application.Services.IInventoryService;

namespace NettoRetail.Application.CQRS.OfferSections.Query;

public record GetSearchSuggestionsQuery(SearchSuggestionsRequest Request) : IRequest<SearchSuggestionsResponse>;

public class GetSearchSuggestionsQueryHandler(
    IInventoryService inventoryService,
    IBrandService brandService,
    ICategoryLevelManagementService categoryLevelManagementService)
    : IRequestHandler<GetSearchSuggestionsQuery, SearchSuggestionsResponse>
{
    public async Task<SearchSuggestionsResponse> Handle(GetSearchSuggestionsQuery request, CancellationToken cancellationToken)
    {
        var query = request.Request.Query?.Trim();
        var page = request.Request.PageModel ?? new PaginationRequest() { PageNumber = 1, PageSize = 10 };

        if (string.IsNullOrWhiteSpace(query))
        {
            return new SearchSuggestionsResponse
            {
                Query = query
            };
        }

        var brandResponses = await brandService.GetShopBrandsBySearchAsync(query, request.Request.ShopId, page, cancellationToken);

        var categoryResponses = await categoryLevelManagementService.GetShopCategoriesBySearchAsync(
            query, request.Request.ShopId, page, cancellationToken);

        var productResponses = await inventoryService.GetShopItemsBySearchAsync(query, request.Request.ShopId, page, cancellationToken);

        return new SearchSuggestionsResponse
        {
            Query = query,
            Brands = brandResponses,
            Categories = categoryResponses,
            Products = productResponses
        };
    }
}
