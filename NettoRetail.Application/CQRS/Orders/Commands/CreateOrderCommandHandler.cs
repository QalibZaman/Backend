using MapsterMapper;
using MediatR;
using NettoRetail.Application.Exceptions;
using NettoRetail.Application.Models.Order;
using NettoRetail.Application.Models.Response;
using NettoRetail.DataAccess.Specifications.Staff;
using NettoRetail.DataAccess.Specifications.Customer;
using NettoRetail.DataAccess.Specifications.Inventory;
using NettoRetail.Domain.Entities;
using NettoRetail.Domain.Entities.JunctionEntities;
using NettoRetail.Shared.Constants;
using NettoRetail.Shared.Interfaces.Repositories;
using NettoRetail.Shared.Interfaces.Utilities;

namespace NettoRetail.Application.CQRS.Orders.Commands;

public record CreateOrderCommand(CreateOrderModel Model, int ShopId, Guid UserId) : IRequest<OrderResponse>;

public class CreateOrderCommandHandler(
        IOrderRepository orderRepository,
        ICustomerRepository customerRepository,
        IStaffRepository staffRepository,
        IInventoryItemRepository inventoryItemRepository,
        IUnitOfWork unitOfWork,
        IMapper mapper) : IRequestHandler<CreateOrderCommand, OrderResponse>
{
    public async Task<OrderResponse> Handle(CreateOrderCommand request, CancellationToken cancellationToken)
    {
        var requestedQuantities = BuildRequestedQuantities(request.Model.Products);
        var inventoryItems = await GetInventoryItemsAsync(requestedQuantities.Keys, request.ShopId, cancellationToken);

        using var transaction = unitOfWork.BeginTransaction();

        try
        {
            ReserveInventory(inventoryItems, requestedQuantities);

            var creator = await GetCreatorAsync(request.UserId, cancellationToken);
            var customer = await GetCustomerAsync(request.Model.CustomerId, request.ShopId, cancellationToken);

            await inventoryItemRepository.UpdateManyAsync(inventoryItems, cancellationToken);
            var data = await AddOrderAsync(request, customer, creator, requestedQuantities, cancellationToken);

            transaction.Commit();

            return data;
        }
        catch (Exception)
        {
            transaction.Rollback();
            throw;
        }
    }

    private static IReadOnlyDictionary<Guid, int> BuildRequestedQuantities(ICollection<OrderProductDto> products) =>
        products
            .GroupBy(product => product.InventoryItemId)
            .ToDictionary(group => group.Key, group => group.Sum(x => x.Amount));

    private async Task<List<InventoryItemEntity>> GetInventoryItemsAsync(
        IEnumerable<Guid> itemIds,
        int shopId,
        CancellationToken cancellationToken)
    {
        var ids = itemIds.ToArray();

        var getItemsByIdsSpecification = new GetInventoryItemsByIdsAndShopSpecification(ids, shopId);
        var inventoryItemsAsync = inventoryItemRepository
            .GetRangeByIdsAsync(getItemsByIdsSpecification, cancellationToken)
            ?? throw new NotFoundException(HandlerConstants.Inventory.InventoryItemsNotFoundMessage);

        var inventoryItems = new List<InventoryItemEntity>();
        await foreach (var item in inventoryItemsAsync)
        {
            inventoryItems.Add(item);
        }

        if (inventoryItems.Count == 0 || inventoryItems.Count != ids.Length)
        {
            throw new NotFoundException(HandlerConstants.Inventory.InventoryItemsNotFoundMessage);
        }

        return inventoryItems;
    }

    private static void ReserveInventory(
        IEnumerable<InventoryItemEntity> inventoryItems,
        IReadOnlyDictionary<Guid, int> requestedQuantities)
    {
        foreach (var item in inventoryItems)
        {
            if (!requestedQuantities.TryGetValue(item.Id, out var requestedQuantity))
            {
                continue;
            }

            if (item.AvailableAmount < requestedQuantity)
            {
                throw new ConflictException($"Item is not available in this quantity. (Available {item.AvailableAmount})");
            }

            item.AvailableAmount -= requestedQuantity;
            item.ReservedAmount += requestedQuantity;
        }
    }



    private async Task<StaffEntity> GetCreatorAsync(Guid userId, CancellationToken cancellationToken)
    {
        var staffByIdSpecification = new GetStaffByIdSpecification(userId);
        return await staffRepository.GetAsync(staffByIdSpecification, cancellationToken)
               ?? throw new NotFoundException(string.Format(HandlerConstants.Account.AccountNotFoundMessage, userId));
    }

    private async Task<CustomerEntity> GetCustomerAsync(Guid customerId, int shopId, CancellationToken cancellationToken)
    {
        var customerByIdSpecification = new GetCustomerByIdAndShopIdSpecification(customerId, shopId);
        return await customerRepository.GetWithRelatedDataAsync(customerByIdSpecification, cancellationToken)
               ?? throw new NotFoundException("There is no customer with such id.");
    }

    private async Task<OrderResponse> AddOrderAsync(
        CreateOrderCommand request,
        CustomerEntity customer,
        StaffEntity creator,
        IReadOnlyDictionary<Guid, int> requestedQuantities,
        CancellationToken cancellationToken)
    {
        var order = mapper.Map<OrderEntity>(request.Model);
        order.ShopId = request.ShopId;
        order.Creator = creator.FullName;

        var orderItems = requestedQuantities.Select(item =>
            new OrderItemEntity { OrderId = order.Id, InventoryItemId = item.Key, Quantity = item.Value }).ToList();
        order.OrderItems = orderItems;

        var result = await orderRepository.AddAsync(order, cancellationToken);
        var data = mapper.Map<OrderResponse>(result);
        data.CustomerEmail = customer.Email;
        data.CustomerFirstName = customer.FirstName;
        data.CustomerLastName = customer.LastName;

        return data;
    }
}
