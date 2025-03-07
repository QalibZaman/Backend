using Mapster;
using MediatR;
using NettoRetail.Application.Exceptions;
using NettoRetail.Application.Models.Category;
using NettoRetail.Application.Models.InventoryItem;
using NettoRetail.DataAccess.Specifications;
using NettoRetail.Domain.Entities;
using NettoRetail.Shared.Interfaces.Repositories;

namespace NettoRetail.Application.CQRS.InventoryItems.Commands;

public record CreateInventoryItemCommand(AddInventoryItemModel Model) : IRequest<AddInventoryItemModel>;

public class CreateInventoryItemCommandHandler(IInventoryItemRepository inventoryItemRepository)
    : IRequestHandler<CreateInventoryItemCommand, AddInventoryItemModel>
{
    public async Task<AddInventoryItemModel> Handle(CreateInventoryItemCommand request, CancellationToken cancellationToken)
    {
        var productByNameSpecification = new GetProductByUpcSpecification(request.Model.ProductUpc);
        var product = await inventoryItemRepository.GetAsync(productByNameSpecification, cancellationToken);

        if (product != null)
        {
            throw new ConflictException("This product UPC is already registered.");
        }

        var inventoryItem = request.Model.Adapt<InventoryItemEntity>();

        var responseModel = await inventoryItemRepository.AddAsync(inventoryItem, cancellationToken);

        var response = responseModel.Adapt<AddInventoryItemModel>();

        return response;
    }
}