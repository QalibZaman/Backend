using Mapster;
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
        IAccountRepository accountRepository,
        IInventoryItemRepository inventoryItemRepository,
        IUnitOfWork unitOfWork) : IRequestHandler<CreateOrderCommand, OrderResponse>
{
    public async Task<OrderResponse> Handle(CreateOrderCommand request, CancellationToken cancellationToken)
    {
        var itemsIdsQuantityDictionary = request.Model.Products;
        var inventoryItemsIds = itemsIdsQuantityDictionary.Keys;

        var getItemsByIdsSpecification = new GetItemsByIdsSpecification(inventoryItemsIds);
        var inventoryItems = inventoryItemRepository.GetRangeByIdsAsync(getItemsByIdsSpecification, cancellationToken) ?? throw new NotFoundException("There are no such items.");

        var itemsList = new List<InventoryItemEntity>();

        var transaction = unitOfWork.BeginTransaction();

        try
        {
            await foreach (var item in inventoryItems)
            {
                if (itemsIdsQuantityDictionary.TryGetValue(item.Id, out var requestedQuantity))
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

            var accountByIdSpecification = new GetAccountByIdSpecification(request.UserId);
            var account = await accountRepository.GetAsync(accountByIdSpecification, cancellationToken);

            if (request.Model.CustomerId == null)
            {
                var accountByEmailSpecification = new GetAccountByEmailSpecification(request.Model.Email);
                var isAccountEmailExists = await accountRepository.Exists(accountByEmailSpecification, cancellationToken);

                if (isAccountEmailExists)
                {
                    throw new ConflictException("This Email is taken.");
                }

                await inventoryItemRepository.UpdateBulkAsync(itemsList, cancellationToken);
                var dataWithoutExistingCustomer = await AddOrderAsync(request, null, account!, itemsIdsQuantityDictionary, cancellationToken);

                transaction.Commit();

                return dataWithoutExistingCustomer;
            }

            var customerByIdSpecification = new GetCustomerByIdSpecification(request.Model.CustomerId);
            var customer = await customerRepository.GetWithRelatedDataAsync(customerByIdSpecification, cancellationToken) ?? throw new NotFoundException("There is no customer with such id.");

            await inventoryItemRepository.UpdateBulkAsync(itemsList, cancellationToken);
            var data = await AddOrderAsync(request, customer, account!, itemsIdsQuantityDictionary, cancellationToken);

            transaction.Commit();

            return data;
        }
        catch (Exception)
        {
            transaction.Rollback();
            throw;
        }
    }

    private async Task<OrderResponse> AddOrderAsync(
        CreateOrderCommand request,
        CustomerEntity? customer,
        AccountEntity account,
        IDictionary<Guid, int> itemsIdsQuantityDictionary,
        CancellationToken cancellationToken)
    {
        var order = request.Model.Adapt<OrderEntity>();
        order.ShopId = request.ShopId;
        order.Creator = $"{account.FirstName} {account.LastName}";

        if (customer != null)
        {
            order.CustomerEmail = customer!.Account.Email;
            order.CustomerFirstName = customer.Account.FirstName;
            order.CustomerLastName = customer.Account.LastName;
        }

        var orderItems = itemsIdsQuantityDictionary.Select(x => new OrderItemEntity { OrderId = order.Id, ItemId = x.Key, Quantity = x.Value }).ToList();
        order.OrderItems = orderItems;

        var result = await orderRepository.AddAsync(order, cancellationToken);
        var data = result.Adapt<OrderResponse>();

        return data;
    }
}