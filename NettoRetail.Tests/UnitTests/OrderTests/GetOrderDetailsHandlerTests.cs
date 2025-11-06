using NettoRetail.Application.CQRS.Orders.Queries;
using NettoRetail.Application.Exceptions;
using NettoRetail.Application.Models.Response;
using NettoRetail.DataAccess.Specifications.Order;
using NettoRetail.DataAccess.Specifications.OrderItems;
using NettoRetail.Domain.Entities;
using NettoRetail.Shared.Interfaces.Repositories;
using NSubstitute;

namespace NettoRetail.Tests.UnitTests.OrderTests
{
    public class GetOrderDetailsHandlerTests
    {
        private readonly IOrderRepository _orderRepository;
        private readonly IOrderItemRepository _orderItemRepository;
        private readonly GetOrderDetailsQueryHandler _handler;

        public GetOrderDetailsHandlerTests()
        {
            _orderRepository = Substitute.For<IOrderRepository>();
            _orderItemRepository = Substitute.For<IOrderItemRepository>();
            _handler = new GetOrderDetailsQueryHandler(_orderRepository, _orderItemRepository);
        }

        [Fact]
        public async Task Handle_ShouldThrowNotFoundException_WhenOrderDoesNotExist()
        {
            // Arrange
            var nonExistingId = -1;

            _orderRepository.GetAsync(Arg.Any<GetOrdersByIdSpecification>(), Arg.Any<CancellationToken>())
                .Returns((OrderEntity?)null);

            // Act & Assert
            await Assert.ThrowsAsync<NotFoundException>(() =>
                _handler.Handle(new GetOrderDetailsQuery(nonExistingId), CancellationToken.None));
        }

        [Fact]
        public void Total_ShouldReturnCorrectSum_WhenOrderItemsPresent()
        {
            // Arrange
            var order = new OrderResponse
            {
                Id = 9,
                CustomerFirstName = "John",
                CustomerLastName = "Doe",
                CustomerEmail = "john.doe@gmail.com",
                CreationDate = DateTime.Parse("2025-10-30T13:35:21.071677Z"),
                DeliveryType = "Courier",
                Creator = "System",
                OrderStatus = "New",
                CustomerId = Guid.Parse("d2ee9235-48d1-4fc6-80c8-a68e2553b6d9"),
                OrderItems = new List<OrderItemResponse>
                {
                    new() { ProductName = "Air Max Shoes", ProductUPC = "123456789012", AvailableAmount = 50, Price = 149.99m, Quantity = 2 },
                    new() { ProductName = "Wireless Mouse", ProductUPC = "123456789019", AvailableAmount = 100, Price = 25.99m, Quantity = 1 },
                    new() { ProductName = "Mechanical Keyboard", ProductUPC = "987654321098", AvailableAmount = 50, Price = 89.5m, Quantity = 1 }
                },
                TotalSale = 0m
            };

            // Act
            var total = order.Total;

            // Assert
            Assert.Equal(415.47m, total);
            Assert.Equal(3, order.OrderItems.Count());
        }

        [Fact]
        public void Total_ShouldBeZero_WhenNoItems()
        {
            var order = new OrderResponse
            {
                Id = 1,
                CustomerFirstName = "Empty",
                CustomerLastName = "Order",
                CustomerEmail = "empty@example.com",
                CreationDate = DateTime.UtcNow,
                DeliveryType = "Courier",
                Creator = "System",
                OrderStatus = "New",
                CustomerId = Guid.NewGuid(),
                OrderItems = new List<OrderItemResponse>(),
                TotalSale = 0
            };

            Assert.Equal(0m, order.Total);
        }

        [Fact]
        public async Task Handle_ShouldTreatNullOrMissingOrderItems_AsEmptyCollection()
        {
            // Arrange
            var order = new OrderEntity
            {
                Id = 10,
                CreationDate = DateTime.UtcNow,
                Creator = "System",
                OrderStatus = Domain.Enums.OrderStatusEnum.New,
                DeliveryType = Domain.Enums.OrderDeliveryTypeEnum.Courier,
                CustomerId = Guid.NewGuid(),
                Customer = new CustomerEntity
                {
                    AccountId = Guid.NewGuid(),
                    Account = new AccountEntity
                    {
                        Id = Guid.NewGuid(),
                        FirstName = "Jane",
                        LastName = "Doe",
                        Email = "jane.doe@example.com",
                        PasswordHash = "hashed123",
                        RoleId = 1,
                    }
                },
                OrderItems = null!
            };

            _orderRepository
                .GetAsync(Arg.Any<GetOrdersByIdSpecification>(), Arg.Any<CancellationToken>())
                .Returns(order);

            _orderItemRepository
                .GetManyBySpecAsync(Arg.Any<GetOrderItemsByOrderIdSpecification>(), Arg.Any<CancellationToken>())
                .Returns(Enumerable.Empty<NettoRetail.Domain.Entities.JunctionEntities.OrderItemEntity>());

            var query = new GetOrderDetailsQuery(order.Id);

            // Act
            var result = await _handler.Handle(query, CancellationToken.None);

            // Assert
            Assert.NotNull(result);
            Assert.Empty(result.OrderItems);
            Assert.Equal(order.Id, result.Id);
            Assert.Equal(0m, result.Total);
        }

        [Fact]
        public void Handle_ShouldIgnoreItemsWithZeroQuantity_WhenCalculatingTotal()
        {
            // Arrange
            var order = new OrderResponse
            {
                Id = 20,
                CustomerFirstName = "Test",
                CustomerLastName = "User",
                CustomerEmail = "test.user@example.com",
                CreationDate = DateTime.UtcNow,
                DeliveryType = "Courier",
                Creator = "System",
                OrderStatus = "New",
                CustomerId = Guid.NewGuid(),
                OrderItems = new List<OrderItemResponse>
                {
                    new() { ProductUPC = "123456789012", ProductName = "Item1", AvailableAmount = 10, Price = 100m, Quantity = 2 },
                    new() { ProductUPC = "123456789012", ProductName = "Item2", AvailableAmount = 10, Price = 50m, Quantity = 0 },
                    new() { ProductUPC = "123456789012", ProductName = "Item3", AvailableAmount = 10, Price = 25m, Quantity = 1 }
                }
            };

            // Act
            var total = order.Total;

            // Assert
            Assert.Equal(225m, total);
        }

        [Fact]
        public void Handle_ShouldHandleZeroPriceItems_TotalUnchanged()
        {
            // Arrange
            var order = new OrderResponse
            {
                Id = 30,
                CustomerFirstName = "Zero",
                CustomerLastName = "Price",
                CustomerEmail = "zero.price@example.com",
                CreationDate = DateTime.UtcNow,
                DeliveryType = "Courier",
                Creator = "System",
                OrderStatus = "New",
                CustomerId = Guid.NewGuid(),
                OrderItems = new List<OrderItemResponse>
                {
                    new() { ProductUPC = "123456789012", ProductName = "Free Gift", AvailableAmount = 10, Price = 0m, Quantity = 1 },
                    new() { ProductUPC = "123456789012", ProductName = "Paid Item", AvailableAmount = 10, Price = 100m, Quantity = 2 },
                    new() { ProductUPC = "123456789012", ProductName = "Zero Price Bulk", AvailableAmount = 10, Price = 0m, Quantity = 5 }
                }
            };

            // Act
            var total = order.Total;

            // Assert
            Assert.Equal(200m, total);
        }

        [Fact]
        public void Handle_ShouldSupportLargeQuantities_WithoutOverflow()
        {
            // Arrange
            var order = new OrderResponse
            {
                Id = 40,
                CustomerFirstName = "Big",
                CustomerLastName = "Order",
                CustomerEmail = "big.order@example.com",
                CreationDate = DateTime.UtcNow,
                DeliveryType = "Courier",
                Creator = "System",
                OrderStatus = "New",
                CustomerId = Guid.NewGuid(),
                OrderItems = new List<OrderItemResponse>
                {
                    new() { ProductUPC = "123456789012", ProductName = "Bulk Item", AvailableAmount = int.MaxValue, Price = 9999.99m, Quantity = 100000 },
                    new() { ProductUPC = "123456789012", ProductName = "Extra Bulk", AvailableAmount = int.MaxValue, Price = 5000m, Quantity = 50000 }
                }
            };

            // Act
            var total = order.Total;

            // Assert
            Assert.True(total > 0);
            Assert.Equal(9999.99m * 100000 + 5000m * 50000, total);
        }
    }
}
