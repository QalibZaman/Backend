using Mapster;
using MediatR;
using NettoRetail.Application.Exceptions;
using NettoRetail.Application.Models.InventoryItem;
using NettoRetail.DataAccess.Repositories;
using NettoRetail.DataAccess.Specifications;
using NettoRetail.Domain.Entities;
using NettoRetail.Shared.Interfaces.Repositories;

namespace NettoRetail.Application.CQRS.Users.Commands;

public record CreateInventoryItemCommand(InventoryItemModel Model) : IRequest;

public class CreateInventoryItemCommandHandler(IInventoryItemRepository inventoryItemRepository)
    : IRequestHandler<CreateInventoryItemCommand>
{
    public async Task Handle(CreateInventoryItemCommand request, CancellationToken cancellationToken)
    {
        var productByNameSpecification = new GetProductByUpcSpecification(request.Model.ProductUpc);
        var product = await inventoryItemRepository.GetAsync(productByNameSpecification, cancellationToken);

        if (product != null)
        {
            throw new ConflictException("This product UPC is already registered.");
        }

        var inventoryItem = request.Model.Adapt<InventoryItemEntity>();

        await inventoryItemRepository.AddAsync(inventoryItem, cancellationToken);
    }
}