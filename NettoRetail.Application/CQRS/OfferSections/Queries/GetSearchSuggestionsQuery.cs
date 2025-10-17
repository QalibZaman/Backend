using MediatR;
using NettoRetail.Application.Models.Request;
using NettoRetail.Application.Models.Request.Offer;
using NettoRetail.Application.Models.Response;
using NettoRetail.DataAccess.Specifications.Brands;
using NettoRetail.DataAccess.Specifications.Categories;
using NettoRetail.DataAccess.Specifications.Inventory;
using NettoRetail.Domain.Entities;
using NettoRetail.Shared.Interfaces.Repositories;
using NettoRetail.Shared.Specifications;

namespace NettoRetail.Application.CQRS.OfferSections.Query;

public record GetSearchSuggestionsQuery(SearchSuggestionsRequest Request) : IRequest<SearchSuggestionsResponse>;

public class GetSearchSuggestionsQueryHandler(
    IInventoryItemRepository inventoryItemRepository,
    ICategoryRepository categoryRepository,
    IBrandRepository brandRepository)
    : IRequestHandler<GetSearchSuggestionsQuery, SearchSuggestionsResponse>
{
    public async Task<SearchSuggestionsResponse> Handle(GetSearchSuggestionsQuery request, CancellationToken cancellationToken)
    {
        var query = request.Request.Query.Trim();
        var page = request.Request.PageModel ?? new PaginationRequest();

        if (string.IsNullOrWhiteSpace(query))
        {
            return new SearchSuggestionsResponse
            {
                Query = query
            };
        }

        var brandSpecBuilder = new SpecificationBuilder<BrandEntity>();
        brandSpecBuilder
            .Add<GetBrandsByPartialNameSpecification>(query)
            .Add<GetBrandsByShopIdSpecification>(request.Request.ShopId);

        var brandSpec = brandSpecBuilder.Build();

        var brands = await brandRepository.GetAllBySpecAsync(brandSpec, cancellationToken);
        var brandResponses = brands
            .Select(b => new BrandResponse
            {
                Id = b.Id,
                Name = b.Name,
                Description = b.Description,
                WebsiteUrl = b.WebsiteUrl
            })
            .Skip((page.PageNumber - 1) * page.PageSize)
            .Take(page.PageSize)
            .ToList();
        var brandTotal = brands.Count();

        var categorySpecBuilder = new SpecificationBuilder<CategoryEntity>();
        categorySpecBuilder
            .Add<GetCategoriesByPartialNameSpecification>(query)
            .Add<GetCategoriesByShopIdSpecification>(request.Request.ShopId);

        var categorySpec = categorySpecBuilder.Build();

        var categories = await categoryRepository.GetAllBySpecAsync(categorySpec, cancellationToken);
        var categoryResponses = categories
            .Select(c => new CategoryResponse
            {
                Id = c.Id,
                Name = c.Name,
                URL = c.URL
            })
            .Skip((page.PageNumber - 1) * page.PageSize)
            .Take(page.PageSize)
            .ToList();
        var categoryTotal = categories.Count();

        var InventoryItemSpecBuilder = new SpecificationBuilder<InventoryItemEntity>();
        InventoryItemSpecBuilder
            .Add<GetInventoryItemsByPartialNameSpecification>(query)
            .Add<GetInventoryItemsByShopIdSpecification>(request.Request.ShopId)
            .AddInclude(x => x.Brand)
            .AddInclude(x => x.Category);

        var specification = InventoryItemSpecBuilder.Build();
        var products = await inventoryItemRepository.GetAllBySpecAsync(specification, cancellationToken);
        var productResponses = products
            .Select(p => new InventoryItemListResponse
            {
                Id = p.Id,
                ProductName = p.ProductName,
                Price = p.Price,
                AvailableAmount = p.AvailableAmount,
                ProductUpc = p.ProductUpc,
                ProductCategory = p.Category?.Name,
                ReservedAmount = p.ReservedAmount,
                OnHandAmount = p.OnHandAmount,
                PromotionPrice = p.PromotionPrice,
                Status = p.Status.ToString(),
                ProductLocation = p.ProductLocation,
                LastUpdatedDate = p.LastUpdatedDate,
                BrandName = p.Brand?.Name,
                IsPromoted = p.IsPromoted
            })
            .Skip((page.PageNumber - 1) * page.PageSize)
            .Take(page.PageSize)
            .ToList();
        var productTotal = products.Count();

        return new SearchSuggestionsResponse
        {
            Query = query,
            Brands = new PaginatedResponse<BrandResponse> { Items = brandResponses, TotalAmount = brandTotal },
            Categories = new PaginatedResponse<CategoryResponse> { Items = categoryResponses, TotalAmount = categoryTotal },
            Products = new PaginatedResponse<InventoryItemListResponse> { Items = productResponses, TotalAmount = productTotal }
        };
    }
}
