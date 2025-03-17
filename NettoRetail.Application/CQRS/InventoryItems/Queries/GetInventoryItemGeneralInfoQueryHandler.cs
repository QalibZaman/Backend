using Mapster;
using MediatR;
using NettoRetail.Application.Exceptions;
using NettoRetail.Application.Models.Response;
using NettoRetail.DataAccess.Specifications.Inventory;
using NettoRetail.Shared.Interfaces.Repositories;

namespace NettoRetail.Application.CQRS.InventoryItems.Queries;

public record GetInventoryItemGeneralInfoQuery(Guid Id) : IRequest<InventoryItemGeneralInfoResponse>;
public class GetInventoryItemGeneralInfoQueryHandler(
    IInventoryItemRepository inventoryItemRepository,
    ICategoryRepository categoryRepository)
    : IRequestHandler<GetInventoryItemGeneralInfoQuery, InventoryItemGeneralInfoResponse>
{
    public async Task<InventoryItemGeneralInfoResponse> Handle(GetInventoryItemGeneralInfoQuery request, CancellationToken cancellationToken)
    {
        var itemSpecification = new GetItemByIdSpecification(request.Id);
        var item = await inventoryItemRepository.GetAsync(itemSpecification, cancellationToken)
            ?? throw new NotFoundException("There is no inventory item with such id.");

        var categories = await categoryRepository
            .GetCategoryWithHierarchyAsync(item.CategoryId, cancellationToken)
            ?? throw new NotFoundException("Category not found.");

        var response = item.Adapt<InventoryItemGeneralInfoResponse>();

        response.ProductCategories = categories
            .Select(x => x.Name);

        return response;
    }
}
