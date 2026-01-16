using Mapster;
using MediatR;
using NettoRetail.Application.Models.Response;
using NettoRetail.DataAccess.Specifications.Inventory.Variations;
using NettoRetail.Shared.Interfaces.Repositories;

namespace NettoRetail.Application.CQRS.Variations.Queries;

public record GetVariationsByExistenceQuery(bool Exists, Guid ParentId, int ShopId) : IRequest<List<InventoryItemResponse>>;

public class GetVariationsByExistenceQueryHandler(
    IInventoryItemRepository inventoryItemRepository)
    : IRequestHandler<GetVariationsByExistenceQuery, List<InventoryItemResponse>>
{
    public async Task<List<InventoryItemResponse>> Handle(
        GetVariationsByExistenceQuery request,
        CancellationToken cancellationToken)
    {
        var spec = new GetVariationsByParentIdAndExistenceSpecification(request.ParentId, request.Exists);

        var variations = await inventoryItemRepository.GetAllBySpecAsync(spec, cancellationToken);

        var response = variations.Adapt<List<InventoryItemResponse>>();

        return response;
    }
}