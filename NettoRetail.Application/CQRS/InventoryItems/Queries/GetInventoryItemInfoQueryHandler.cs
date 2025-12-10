using Mapster;
using MediatR;
using NettoRetail.Application.Exceptions;
using NettoRetail.Application.Models.Response;
using NettoRetail.Application.Models.Response.Category;
using NettoRetail.DataAccess.Specifications.Inventory;
using NettoRetail.Shared.Interfaces.Repositories;

namespace NettoRetail.Application.CQRS.InventoryItems.Queries;

public record GetInventoryItemFullInfoQuery(Guid Id) : IRequest<InventoryItemInfoFullResponse>;

public class GetInventoryItemInfoQueryHandler(
    IInventoryItemRepository inventoryRepo,
    ICategoryRepository categoryRepo) : IRequestHandler<GetInventoryItemFullInfoQuery, InventoryItemInfoFullResponse>
{
    public async Task<InventoryItemInfoFullResponse> Handle(GetInventoryItemFullInfoQuery request, CancellationToken cancellationToken)
    {
        var spec = new GetItemByIdSpecification(request.Id)
            .AddInclude(i => i.Brand!)
            .AddInclude(i => i.Category)
            .AddInclude(i => i.ProductDetails)
            .AddInclude(i => i.Images);

        var item = await inventoryRepo.GetAsync(spec, cancellationToken) ?? throw new NotFoundException("There is no inventory item with such id.");
        var response = item.Adapt<InventoryItemInfoFullResponse>();

        var chain = await categoryRepo.GetCategoryWithHierarchyAsync(item.CategoryId, cancellationToken);
        response.ProductCategories = chain
            .Select(c => new CategorySummaryResponse { Id = c.Id, Name = c.Name })
            .ToList();

        return response;
    }
}