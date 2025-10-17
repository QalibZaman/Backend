using Mapster;
using MediatR;
using NettoRetail.Application.Exceptions;
using NettoRetail.Application.Models.Response;
using NettoRetail.DataAccess.Specifications.Inventory;
using NettoRetail.Shared.Interfaces.Repositories;

namespace NettoRetail.Application.CQRS.InventoryItems.Queries;

public record GetInventoryItemInfoQuery(Guid Id) : IRequest<InventoryItemInfoResponse>;
public class GetInventoryItemInfoQueryHandler(
    IInventoryItemRepository inventoryItemRepository,
    ICategoryRepository categoryRepository)
    : IRequestHandler<GetInventoryItemInfoQuery, InventoryItemInfoResponse>
{
    public async Task<InventoryItemInfoResponse> Handle(GetInventoryItemInfoQuery request, CancellationToken cancellationToken)
    {
        var specification = new GetItemByIdSpecification(request.Id);
        specification.AddInclude(x => x.Brand!);

        var item = await inventoryItemRepository
           .GetAsync(specification, cancellationToken)
            ?? throw new NotFoundException("There is no inventory item with such id.");

        var categories = await categoryRepository
            .GetCategoryWithHierarchyAsync(item.CategoryId, cancellationToken);

        var response = item.Adapt<InventoryItemInfoResponse>();

        response.ProductCategories = categories.Select(x => x.Name);

        return response;
    }
}
