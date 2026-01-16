using Mapster;
using MediatR;
using NettoRetail.Application.Models.Response;
using NettoRetail.DataAccess.Specifications.Inventory.Variations;
using NettoRetail.Shared.Interfaces.Repositories;

namespace NettoRetail.Application.CQRS.Variations.Queries;

public record GetItemVariationsQuery(Guid ParentId, int ShopId) : IRequest<List<InventoryItemResponse>>;

public class GetItemVariationsQueryHandler(
    IInventoryItemRepository inventoryItemRepository)
    : IRequestHandler<GetItemVariationsQuery, List<InventoryItemResponse>>
{
    public async Task<List<InventoryItemResponse>> Handle(
        GetItemVariationsQuery request,
        CancellationToken cancellationToken)
    {
        var spec = new GetVariationsByParentIdSpecification(request.ParentId);

        var variations = await inventoryItemRepository.GetAllBySpecAsync(spec, cancellationToken);

        var response = variations.Adapt<List<InventoryItemResponse>>();

        return response;
    }
}