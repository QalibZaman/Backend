using NettoRetail.Application.CQRS.Orders.Queries;
using NettoRetail.Application.Exceptions;
using NettoRetail.Application.Models.Response;
using NettoRetail.DataAccess.Specifications.Order;
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
                    new OrderItemResponse
                    {
                        ProductName = "Air Max Shoes",
                        ProductUPC = "123456789012",
                        AvailableAmount = 50,
                        Price = 149.99m,
                        Quantity = 2,
                    },
                    new OrderItemResponse
                    {
                        ProductName = "Wireless Mouse",
                        ProductUPC = "123456789019",
                        AvailableAmount = 100,
                        Price = 25.99m,
                        Quantity = 1,
                    },
                    new OrderItemResponse
                    {
                        ProductName = "Mechanical Keyboard",
                        ProductUPC = "987654321098",
                        AvailableAmount = 50,
                        Price = 89.5m,
                        Quantity = 1,
                    }
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
    }
}
