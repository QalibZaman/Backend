using Mapster;
using MediatR;
using Microsoft.Extensions.Options;
using NettoRetail.Application.Models.InventoryItem;
using NettoRetail.Application.Models.Response;
using NettoRetail.DataAccess.Specifications.Inventory;
using NettoRetail.Domain.Entities;
using NettoRetail.Shared.Interfaces.Repositories;
using NettoRetail.Shared.Options;
using NettoRetail.Shared.Specifications;

namespace NettoRetail.Application.CQRS.InventoryItems.Queries;

public record GetInventoryItemsByUpcOrNameQuery(GetInventoryItemsByUpcOrNameModel Model, int ShopId) : IRequest<ICollection<InventoryItemListResponse>>;
public class GetInventoryItemsByUpcOrNameQueryHandler(
    IInventoryItemRepository inventoryItemRepository,
    IOptions<SearchOptions> searchOptions)
    : IRequestHandler<GetInventoryItemsByUpcOrNameQuery, ICollection<InventoryItemListResponse>>
{
    public async Task<ICollection<InventoryItemListResponse>> Handle(GetInventoryItemsByUpcOrNameQuery request, CancellationToken cancellationToken)
    {
        var searchOptionsValue = searchOptions.Value;
        var specificationBuilder = new SpecificationBuilder<InventoryItemEntity>();

        specificationBuilder.Add<GetInventoryItemsByShopIdSpecification>(request.ShopId);

        if (long.TryParse(request.Model.UpcOrName, out var upc))
        {
            specificationBuilder.Add<GetInventoryItemsByPartialUpcSpecification>(upc);
        }
        else
        {
            specificationBuilder.Add<GetInventoryItemsByPartialNameSpecification>(request.Model.UpcOrName);
        }

        var specification = specificationBuilder.Take(searchOptionsValue.InventoryItemSearchSize).Build();

        var items = await inventoryItemRepository.GetPaginatedAsync(specification, cancellationToken);

        var responseCollection = items.Adapt<List<InventoryItemListResponse>>();

        return responseCollection;
    }
}
