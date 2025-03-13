using Mapster;
using MediatR;
using NettoRetail.Application.Exceptions;
using NettoRetail.Application.Models.InventoryItem;
using NettoRetail.DataAccess.Specifications.Categories;
using NettoRetail.DataAccess.Specifications.Inventory;
using NettoRetail.Domain.Entities;
using NettoRetail.Shared.Interfaces.Repositories;

namespace NettoRetail.Application.CQRS.InventoryItems.Commands;

public record CreateInventoryItemCommand(AddInventoryItemModel Model) : IRequest<InventoryItemResponse>;

public class CreateInventoryItemCommandHandler(IInventoryItemRepository inventoryItemRepository, ICategoryRepository categoryRepository)
    : IRequestHandler<CreateInventoryItemCommand, InventoryItemResponse>
{
    public async Task<InventoryItemResponse> Handle(CreateInventoryItemCommand request, CancellationToken cancellationToken)
    {
        var productByNameSpecification = new GetProductByUpcSpecification(request.Model.ProductUpc);
        var product = await inventoryItemRepository.GetAsync(productByNameSpecification, cancellationToken);

        if (product != null)
        {
            throw new ConflictException("This product UPC is already registered.");
        }

        var categoryByIdSpecification = new GetCategoryByIdSpecification(request.Model.CategoryId);
        var category = await categoryRepository.GetAsync(categoryByIdSpecification, cancellationToken);

        if (category == null)
        {
            throw new NotFoundException($"There is no category with Id: {request.Model.CategoryId}");
        }

        var inventoryItem = request.Model.Adapt<InventoryItemEntity>();

        var responseModel = await inventoryItemRepository.AddAsync(inventoryItem, cancellationToken);

        var response = responseModel.Adapt<InventoryItemResponse>();
        response.ProductCategory = category.Name;

        return response;
    }
}