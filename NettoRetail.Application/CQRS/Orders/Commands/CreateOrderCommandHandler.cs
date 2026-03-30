using MapsterMapper;
using MediatR;
using NettoRetail.Application.Exceptions;
using NettoRetail.Application.Models.Order;
using NettoRetail.Application.Models.Response;
using NettoRetail.DataAccess.Specifications.Account;
using NettoRetail.DataAccess.Specifications.Customer;
using NettoRetail.DataAccess.Specifications.Inventory;
using NettoRetail.Domain.Entities;
using NettoRetail.Domain.Entities.JunctionEntities;
using NettoRetail.Shared.Interfaces.Repositories;
using NettoRetail.Shared.Interfaces.Utilities;

namespace NettoRetail.Application.CQRS.Orders.Commands;

public record CreateOrderCommand(CreateOrderModel Model, int ShopId, Guid UserId) : IRequest<OrderResponse>;

public class CreateOrderCommandHandler(
        IOrderRepository orderRepository,
        ICustomerRepository customerRepository,
        IInventoryItemRepository inventoryItemRepository,
        IUnitOfWork unitOfWork,
        IMapper mapper) : IRequestHandler<CreateOrderCommand, OrderResponse>
{
    public async Task<OrderResponse> Handle(CreateOrderCommand request, CancellationToken cancellationToken)
    {
        var requestedQuantities = BuildRequestedQuantities(request.Model.Products);
        var inventoryItems = await GetInventoryItemsAsync(requestedQuantities.Keys, cancellationToken);

        using var transaction = unitOfWork.BeginTransaction();

        try
        {
            ReserveInventory(inventoryItems, requestedQuantities);

            var account = await GetCustomerAsync(request.UserId, cancellationToken);
            var customer = await GetCustomerAsync(request.Model.CustomerId, cancellationToken);

            await inventoryItemRepository.UpdateManyAsync(inventoryItems, cancellationToken);
            var data = await AddOrderAsync(request, customer, requestedQuantities, cancellationToken);

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
        CancellationToken cancellationToken)
    {
        var ids = itemIds.ToArray();

        var getItemsByIdsSpecification = new GetItemsByIdsSpecification(ids);
        var inventoryItemsAsync = inventoryItemRepository
            .GetRangeByIdsAsync(getItemsByIdsSpecification, cancellationToken)
            ?? throw new NotFoundException("There are no such item.");

        var inventoryItems = new List<InventoryItemEntity>();
        await foreach (var item in inventoryItemsAsync)
        {
            inventoryItems.Add(item);
        }

        if (inventoryItems.Count == 0 || inventoryItems.Count != ids.Length)
        {
            throw new NotFoundException("There are no such item.");
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
                throw new ConflictException("Available amount of items is less then requested.");
            }

            item.AvailableAmount -= requestedQuantity;
            item.ReservedAmount += requestedQuantity;
        }
    }



    private async Task<CustomerEntity> GetCustomerAsync(Guid customerId, CancellationToken cancellationToken)
    {
        var customerByIdSpecification = new GetCustomerByIdSpecification(customerId);
        return await customerRepository.GetWithRelatedDataAsync(customerByIdSpecification, cancellationToken)
               ?? throw new NotFoundException("There is no customer with such id.");
    }

    private async Task<OrderResponse> AddOrderAsync(
        CreateOrderCommand request,
        CustomerEntity customer,
        IReadOnlyDictionary<Guid, int> requestedQuantities,
        CancellationToken cancellationToken)
    {
        var order = mapper.Map<OrderEntity>(request.Model);
        order.ShopId = request.ShopId;
        order.Creator = $"{customer.FirstName} {customer.LastName}";

        var orderItems = requestedQuantities.Select(item =>
            new OrderItemEntity { OrderId = order.Id, ItemId = item.Key, Quantity = item.Value }).ToList();
        order.OrderItems = orderItems;

        var result = await orderRepository.AddAsync(order, cancellationToken);
        var data = mapper.Map<OrderResponse>(result);
        data.CustomerEmail = customer.Email;
        data.CustomerFirstName = customer.FirstName;
        data.CustomerLastName = customer.LastName;

        return data;
    }
}
