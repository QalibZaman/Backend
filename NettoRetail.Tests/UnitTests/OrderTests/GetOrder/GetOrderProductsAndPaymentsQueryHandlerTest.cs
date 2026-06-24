using MapsterMapper;
using NettoRetail.Application.CQRS.Orders.Queries;
using NettoRetail.Application.Exceptions;
using NettoRetail.Application.Models.Response;
using NettoRetail.Domain.Entities;
using NettoRetail.Domain.Entities.JunctionEntities;
using NettoRetail.Domain.Enums;
using NettoRetail.Shared.Constants;
using NettoRetail.Shared.Interfaces.Repositories;
using NettoRetail.Shared.Specifications;
using NettoRetail.Tests.Helpers;
using NSubstitute;

namespace NettoRetail.Tests.UnitTests.OrderTests.GetOrder;

public class GetOrderProductsQueryHandlerTest
{
    
        private readonly IOrderRepository _orderRepository;
        private readonly IProductReviewRepository _productReviewRepository;
        private readonly GetOrderProductsQueryHandler _handler;
        private readonly IMapper _mapper;

        public GetOrderProductsQueryHandlerTest()
        {
            _orderRepository = Substitute.For<IOrderRepository>();
            _productReviewRepository = Substitute.For<IProductReviewRepository>();

            _productReviewRepository
                .Exists(
                    Arg.Any<Specification<ProductReviewEntity>>(),
                    Arg.Any<CancellationToken>())
                .Returns(false);

            _mapper = MapperFactory.Create();

            _handler = new GetOrderProductsQueryHandler(
                _orderRepository,
                _productReviewRepository,
                _mapper);
        }

        [Fact]
    public async Task Handler_ShouldThrownNotFoundException_WhenOrderNotFound()
    {
        // Arrange
        var orderId = 12;
        var userId = 51;

        _orderRepository
            .GetOrderWithInventoryItemsAsync(Arg.Any<Specification<OrderEntity>>(), Arg.Any<CancellationToken>())
            .Returns((OrderEntity?)null);

        var query = new GetOrderProductsQuery(orderId, userId);
        // Act & Assert
        var ex = await Record.ExceptionAsync(() => _handler.Handle(query, CancellationToken.None));

        // Assert
        Assert.NotNull(ex);
        Assert.IsType<NotFoundException>(ex);
        Assert.Equal(HandlerConstants.Order.OrderNotFoundMessage, ex.Message);
    }

    [Fact]
    public async Task Handler_ShouldReturnOrderResponse_WhenOrderExists()
    {
        // Arrange
        var orderId = 9;
        var userId = 51;
        var order = BuildTestOrder(orderId);

        _orderRepository
            .GetOrderWithInventoryItemsAsync(Arg.Any<Specification<OrderEntity>>(), Arg.Any<CancellationToken>())
            .Returns(order);

        var query = new GetOrderProductsQuery(orderId, userId);

        // Act
        var result = await _handler.Handle(query, CancellationToken.None);

        // Assert
        Assert.NotNull(result);
        Assert.IsType<OrderProductsResponse>(result);

        await _orderRepository
            .Received(1)
            .GetOrderWithInventoryItemsAsync(Arg.Any<Specification<OrderEntity>>(), Arg.Any<CancellationToken>());
    }

    private static OrderEntity BuildTestOrder(int id)
    {
        var inventoryItem = new InventoryItemEntity
        {
            Id = Guid.NewGuid(),
            ProductName = "Test Product",
            ProductUpc = "123456789000",
            Description = "Test product description",
            CategoryId = 1,
            LastUpdatedDate = DateTime.UtcNow,
            IsActive = true,
            AvailableAmount = 10,
            ReservedAmount = 0,
            IncomingAmount = 3,
            OnHandAmount = 10,
            ProductLocation = "Shelf A",
            ShopId = 1,
            BrandId = 1,
            Price = 20.0m,
            PromotionPrice = 10.0m,
            IsPromoted = true,
            Status = InventoryItemStatusEnum.Available,
            Brand = new BrandEntity { Id = 1, Name = "TestBrand", ShopId = 1 },
            Category = new CategoryEntity
                { Id = 1, Name = "Cat", URL = "cat", Visibility = true, Level = 1, ShopId = 1 }
        };

        var orderItem = new OrderItemEntity
        {
            Id = Guid.NewGuid(),
            InventoryItemId = inventoryItem.Id,
            InventoryItem = inventoryItem,
            Quantity = 2,
            OrderId = id,
        };

        List<OrderItemEntity> itemsList = [orderItem];

        var customer = new CustomerEntity
        {
            Id = Guid.NewGuid(),
            FirstName = "John",
            LastName = "Doe",
            Email = "john.doe@example.com",
            PasswordHash = "irrelevant",
            IsActive = true,
            RoleId = 1
        };

        var delivery = new DeliveryEntity
        {
            Id = Guid.NewGuid(),
            CustomerId = customer.Id,
            DeliveryType = OrderDeliveryTypeEnum.Courier,
            DeliveryFee = 12,
            PaymentTotal = 100,
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow
            
        };
        var order = new OrderEntity
        {
            Id = id,
            CreationDate = DateTime.UtcNow,
            Creator = "Tester",
            OrderStatus = OrderStatusEnum.Processing,
            Delivery = delivery,
            CustomerId = customer.Id,
            Customer = customer,
            ShopId = 1,
            OrderItems = itemsList
        };

        return order;
    }
}