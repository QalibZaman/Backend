using Mapster;
using MediatR;
using NettoRetail.Application.Exceptions;
using NettoRetail.Application.Models.Response;
using NettoRetail.Application.Models.Response.Category;
using NettoRetail.DataAccess.Specifications.Inventory;
using NettoRetail.Shared.Interfaces.Repositories;
using System.Threading;

public record GetInventoryItemInfoQuery(Guid Id) : IRequest<InventoryItemInfoResponse>;

public class GetInventoryItemInfoQueryHandler(
    IInventoryItemRepository inventoryRepo,
    ICategoryRepository categoryRepo) : IRequestHandler<GetInventoryItemInfoQuery, InventoryItemInfoResponse>
{
    public async Task<InventoryItemInfoResponse> Handle(GetInventoryItemInfoQuery request, CancellationToken cancellationToken)
    {
        var spec = new GetItemByIdSpecification(request.Id)
            .AddInclude(i => i.Brand!)
            .AddInclude(i => i.Category!);

        var item = await inventoryRepo.GetAsync(spec, cancellationToken) ?? throw new NotFoundException("There is no inventory item with such id.");
        var response = item.Adapt<InventoryItemInfoResponse>();

        var chain = await categoryRepo.GetCategoryWithHierarchyAsync(item.CategoryId, cancellationToken);
        response.ProductCategories = chain
            .Select(c => new CategorySummaryResponse { Id = c.Id, Name = c.Name })
            .ToList();

        return response;
    }
}