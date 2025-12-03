using NettoRetail.Application.CQRS.Orders.Queries;
using NettoRetail.Application.Exceptions;
using NettoRetail.Application.Models.Response;
using NettoRetail.Domain.Entities;
using NettoRetail.Domain.Entities.JunctionEntities;
using NettoRetail.Domain.Enums;
using NettoRetail.Shared.Interfaces.Repositories;
using NettoRetail.Shared.Specifications;
using NSubstitute;

namespace NettoRetail.Tests.UnitTests.OrderTests;

public class GetOrderProductsQueryHandlerTest
{
    private readonly IOrderRepository _orderRepository;
    private readonly GetOrderProductsQueryHandler _handler;

    public GetOrderProductsQueryHandlerTest()
    {
        _orderRepository = Substitute.For<IOrderRepository>();
        _handler = new GetOrderProductsQueryHandler(_orderRepository);
    }

    [Fact]
    public async Task Handler_ShouldThrownNotFoundException_WhenOrderNotFound()
    {
        // Arrange
        var orderId = 12;

        _orderRepository
            .GetOrderForProducts(Arg.Any<Specification<OrderEntity>>(), Arg.Any<CancellationToken>())
            .Returns((OrderEntity?)null);

        var query = new GetOrderProductsQuery(orderId);
        // Act & Assert
        var ex = await Record.ExceptionAsync(() => _handler.Handle(query, CancellationToken.None));

        // Assert
        Assert.NotNull(ex);
        Assert.IsType<NotFoundException>(ex);
        Assert.Equal("There is no such order.", ex.Message);
    }

    [Fact]
    public async Task Handler_ShouldReturnOrderResponse_WhenOrderExists()
    {
        // Arrange
        var orderId = 9;
        var order = BuildTestOrder(orderId);

        _orderRepository
            .GetOrderForProducts(Arg.Any<Specification<OrderEntity>>(), Arg.Any<CancellationToken>())
            .Returns(order);

        var query = new GetOrderProductsQuery(orderId);

        // Act
        var result = await _handler.Handle(query, CancellationToken.None);

        // Assert
        Assert.NotNull(result);
        Assert.IsType<OrderProductsResponse>(result);
    }

    private static OrderEntity BuildTestOrder(int id)
    {
        var inventoryItem = new InventoryItemEntity
        {
            Id = Guid.NewGuid(),
            ProductName = "Test Product",
            ProductUpc = 123456789000,
            Description = "Test product description",
            CategoryId = 1,
            LastUpdatedDate = DateTime.UtcNow,
            IsActive = true,
            AvailableAmount = 10,
            ReservedAmount = 0,
            OnHandAmount = 10,
            ProductLocation = "Shelf A",
            ShopId = 1,
            BrandId = 1,
            Price = 20.0m,
            PromotionPrice = 10.0m,
            IsPromoted = true,
            Status = InventoryItemStatusEnum.Available,
            Brand = new BrandEntity { Id = 1, Name = "TestBrand", ShopId = 1 },
            Category = new CategoryEntity { Id = 1, Name = "Cat", URL = "cat", Visibility = true, Level = 1, ShopId = 1 }
        };

        var orderItem = new OrderItemEntity
        {
            ItemId = inventoryItem.Id,
            InventoryItem = inventoryItem,
            Quantity = 2,
            OrderId = id,
        };

        List<OrderItemEntity> itemsList = [orderItem];

        var account = new AccountEntity
        {
            Id = Guid.NewGuid(),
            FirstName = "John",
            LastName = "Doe",
            Email = "john.doe@example.com",
            PasswordHash = "irrelevant",
            RoleId = 1
        };

        var customer = new CustomerEntity
        {
            AccountId = account.Id,
            Account = account,
            IsActive = true
        };

        var order = new OrderEntity
        {
            Id = id,
            CreationDate = DateTime.UtcNow,
            Creator = "Tester",
            OrderStatus = OrderStatusEnum.Processing,
            DeliveryType = OrderDeliveryTypeEnum.Courier,
            CustomerId = customer.AccountId,
            Customer = customer,
            ShopId = 1,
            OrderItems = itemsList
        };

        return order;
    }
}
