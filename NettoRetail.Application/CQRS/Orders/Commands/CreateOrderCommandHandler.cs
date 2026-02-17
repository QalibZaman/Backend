using Mapster;
using MediatR;
using NettoRetail.Application.Exceptions;
using NettoRetail.Application.Models.Order;
using NettoRetail.Application.Models.Response;
using NettoRetail.DataAccess.Repositories;
using NettoRetail.DataAccess.Specifications.Account;
using NettoRetail.DataAccess.Specifications.Categories;
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
        IAccountRepository accountRepository,
        IInventoryItemRepository inventoryItemRepository,
        IUnitOfWork unitOfWork) : IRequestHandler<CreateOrderCommand, OrderResponse>
{
    public async Task<OrderResponse> Handle(CreateOrderCommand request, CancellationToken cancellationToken)
    {
        var products = request.Model.Products;
        var inventoryItemsIds = products.Keys;

        var getItemsByIdsSpecification = new GetItemsByIdsSpecification(inventoryItemsIds);
        var inventoryItems = inventoryItemRepository.GetRangeByIdsAsync(getItemsByIdsSpecification, cancellationToken)
            ?? throw new NotFoundException("There are no such items.");

        var itemsList = new List<InventoryItemEntity>();

        using var transaction = unitOfWork.BeginTransaction();

        await foreach (var item in inventoryItems)
        {
            if (inventoryItemsIds.Contains(item.Id))
                throw new NotFoundException($"The product with ID {item.Id} were not found.");

            if (products.TryGetValue(item.Id, out var requestedQuantity))
            {
                if (item.AvailableAmount < requestedQuantity)
                {
                    throw new ConflictException("Available amount of items is less then requested.");
                }

                item.AvailableAmount -= requestedQuantity;
                item.ReservedAmount += requestedQuantity;
            }

            itemsList.Add(item);
        }

        var account = await GetAccountAsync(request.UserId, cancellationToken);

        var customer = await GetCustomerAsync(request.Model.CustomerId, cancellationToken);

        await inventoryItemRepository.UpdateManyAsync(itemsList, cancellationToken);
        var data = await AddOrderAsync(request, customer, account, products, cancellationToken);

        transaction.Commit();

        return data;
    }

    private async Task<OrderResponse> AddOrderAsync(
        CreateOrderCommand request,
        CustomerEntity customer,
        AccountEntity account,
        IDictionary<Guid, int> products,
        CancellationToken cancellationToken)
    {
        var order = request.Model.Adapt<OrderEntity>();
        order.ShopId = request.ShopId;
        order.Creator = $"{account.FirstName} {account.LastName}";

        var orderItems = products.Select(x => new OrderItemEntity { OrderId = order.Id, ItemId = x.Key, Quantity = x.Value }).ToList();
        order.OrderItems = orderItems;

        var result = await orderRepository.AddAsync(order, cancellationToken);
        var data = result.Adapt<OrderResponse>();
        data.CustomerEmail = customer.Account.Email;
        data.CustomerFirstName = customer.Account.FirstName;
        data.CustomerLastName = customer.Account.LastName;

        return data;
    }

    private async Task<AccountEntity> GetAccountAsync(Guid userId, CancellationToken cancellationToken)
    {
        var specification = new GetAccountByIdSpecification(userId);
        return await accountRepository.GetAsync(specification, cancellationToken)
            ?? throw new MyUnauthorizedException("Account not found.");
    }

    private async Task<CustomerEntity> GetCustomerAsync(Guid customerId, CancellationToken cancellationToken)
    {
        var specification = new GetCustomerByIdSpecification(customerId);
        return await customerRepository.GetWithRelatedDataAsync(specification, cancellationToken)
            ?? throw new NotFoundException($"There is no customer with  such ID:{customerId}");
    }
}
