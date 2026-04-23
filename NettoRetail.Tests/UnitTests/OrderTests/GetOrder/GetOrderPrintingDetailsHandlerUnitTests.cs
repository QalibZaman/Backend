using Microsoft.Extensions.Logging.Abstractions;
using NettoRetail.Application.CQRS.Orders.Queries;
using NettoRetail.Application.Exceptions;
using NettoRetail.Application.Models.InventoryItem;
using NettoRetail.Application.Models.Order;
using NettoRetail.Application.Models.Response;
using NettoRetail.Application.Services.IOrderService;
using NettoRetail.Application.Services.IPdfGenerator;
using NettoRetail.Domain.Entities;
using NettoRetail.Domain.Entities.JunctionEntities;
using NettoRetail.Domain.Enums;
using NettoRetail.Shared.Interfaces.Repositories;
using NettoRetail.Shared.Specifications;
using NSubstitute;
using NSubstitute.ExceptionExtensions;

namespace NettoRetail.Tests.UnitTests.OrderTests.GetOrder
{
    public class GetOrderDetailsForPrintingQueryHandlerTests
    {
        private readonly IOrderRepository _orderRepository;
        private readonly IOrderService _orderService;
        private readonly IPdfGenerator _pdfGenerator;
        private readonly GetOrderDetailsForPrintingQueryHandler _handler;

        public GetOrderDetailsForPrintingQueryHandlerTests()
        {
            _orderRepository = Substitute.For<IOrderRepository>();
            _orderService = Substitute.For<IOrderService>();
            _pdfGenerator = Substitute.For<IPdfGenerator>();
            _handler = new GetOrderDetailsForPrintingQueryHandler(_orderRepository, _orderService, _pdfGenerator,
                NullLogger<GetOrderDetailsForPrintingQueryHandler>.Instance);
        }

        [Fact]
        public async Task Handle_ShouldReturnOrderPrintingResponse_WhenOrderExists()
        {
            // Arrange
            var order = BuildTestOrder(9);
            var model = BuildPrintOrderDetailsModel(order);
            var pdfStream = new MemoryStream(new byte[] { 1, 2, 3 });

            _orderRepository
                .GetOrderForPrinting(Arg.Any<Specification<OrderEntity>>(), Arg.Any<CancellationToken>())
                .Returns(order);

            _orderService
                .BuildPrintOrderDetailsModelAsync(order)
                .Returns(model);

            _pdfGenerator
                .PrintOrderDetailsAsync(model)
                .Returns(pdfStream);

            var query = new GetOrderDetailsForPrintingQuery(order.Id);

            // Act
            var result = await _handler.Handle(query, CancellationToken.None);

            // Assert
            Assert.NotNull(result);
            Assert.IsType<OrderPrintingResponse>(result);
            Assert.Equal($"order_{order.Id}.pdf", result.FileName);
            Assert.NotNull(result.Stream);
        }

        [Fact]
        public async Task Handle_ShouldThrowNotFoundException_WhenOrderIsNull()
        {
            // Arrange
            var orderId = 15;

            _orderRepository
                .GetOrderForPrinting(Arg.Any<Specification<OrderEntity>>(), Arg.Any<CancellationToken>())
                .Returns((OrderEntity?)null);

            var query = new GetOrderDetailsForPrintingQuery(orderId);

            // Act & Assert
            await Assert.ThrowsAsync<NotFoundException>(() => _handler.Handle(query, CancellationToken.None));
        }

        [Fact]
        public async Task Handle_ShouldThrowException_WhenPdfGenerationFails()
        {
            // Arrange
            var order = BuildTestOrder(9);
            var model = BuildPrintOrderDetailsModel(order);

            _orderRepository
                .GetOrderForPrinting(Arg.Any<Specification<OrderEntity>>(), Arg.Any<CancellationToken>())
                .Returns(order);

            _orderService
                .BuildPrintOrderDetailsModelAsync(order)
                .Returns(model);

            _pdfGenerator
                .PrintOrderDetailsAsync(model)
                .Throws(new Exception("PDF error"));

            var query = new GetOrderDetailsForPrintingQuery(order.Id);

            // Act
            var ex = await Record.ExceptionAsync(() => _handler.Handle(query, CancellationToken.None));

            // Assert
            Assert.NotNull(ex);
            Assert.IsType<Exception>(ex);
            Assert.Equal("PDF error", ex.Message);
        }

        private static OrderEntity BuildTestOrder(int id)
        {
            var staff = new StaffEntity
            {
                Id = Guid.NewGuid(),
                FirstName = "John",
                LastName = "Doe",
                Email = "john.doe@example.com",
                PasswordHash = "irrelevant",
                RoleId = 1
            };
            var customer = new CustomerEntity()
            {
                Id = Guid.NewGuid(),
                FirstName = "Customer",
                LastName = "Customer",
                Email = "custo.mer@example.com",
                PasswordHash = "irrelevant",
                RoleId = 2
            };
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
                PromotionPrice = null,
                IsPromoted = false,
                Status = InventoryItemStatusEnum.Available,
                Brand = new BrandEntity { Id = 1, Name = "TestBrand", ShopId = 1 },
                Category = new CategoryEntity
                    { Id = 1, Name = "Cat", URL = "cat", Visibility = true, Level = 1, ShopId = 1 }
            };

            var orderItem = new OrderItemEntity
            {
                ItemId = inventoryItem.Id,
                InventoryItem = inventoryItem,
                Quantity = 2
            };

            var statusHistory = new List<OrderStatusHistoryEntity>
            {
                new OrderStatusHistoryEntity
                {
                    OrderId = id,
                    Id = Guid.NewGuid(),
                    UpdatedAt = DateTime.UtcNow,
                    OldStatus = OrderStatusEnum.New,
                    NewStatus = OrderStatusEnum.Processing,
                    Comment = "",
                    UpdatedBy = staff
                }
            };

            var order = new OrderEntity
            {
                Id = id,
                CreationDate = DateTime.UtcNow,
                Creator = "Tester",
                OrderStatus = OrderStatusEnum.Processing,
                DeliveryType = OrderDeliveryTypeEnum.Courier,
                CustomerId = customer.Id,
                Customer = customer,
                ShopId = 1,
                OrderItems = new List<OrderItemEntity> { orderItem },
                StatusHistory = statusHistory
            };

            orderItem.Order = order;
            return order;
        }

        private static PrintOrderDetailsModel BuildPrintOrderDetailsModel(OrderEntity order)
        {
            return new PrintOrderDetailsModel
            {
                Id = order.Id,
                CreationDate = order.CreationDate,
                CustomerId = order.CustomerId,
                CustomerFirstName = order.Customer.FirstName,
                CustomerLastName = order.Customer.LastName,
                CustomerEmail = order.Customer.Email,
                OrderStatus = order.OrderStatus.ToString(),
                DeliveryType = order.DeliveryType.ToString(),
                TotalProducts = order.OrderItems.Count,
                InventoryItems = order.OrderItems.Select(orderItemEntity => new InventoryItemDto
                {
                    ProductName = orderItemEntity.InventoryItem.ProductName,
                    ProductUpc = orderItemEntity.InventoryItem.ProductUpc,
                    Price = orderItemEntity.InventoryItem.Price,
                    PromotionPrice = orderItemEntity.InventoryItem.PromotionPrice,
                    IsPromoted = orderItemEntity.InventoryItem.IsPromoted,
                    AvailableAmount = orderItemEntity.InventoryItem.AvailableAmount,
                    Quantity = orderItemEntity.Quantity,
                    ReservedAmount = orderItemEntity.InventoryItem.ReservedAmount,
                    OnHandAmount = orderItemEntity.InventoryItem.OnHandAmount
                }).ToList(),
                Subtotal = order.OrderItems.Sum(orderItemEntity =>
                    orderItemEntity.InventoryItem.Price * orderItemEntity.Quantity),
                TotalSale = 0m,
                TotalAmount = order.OrderItems.Sum(orderItemEntity =>
                    orderItemEntity.InventoryItem.Price * orderItemEntity.Quantity)
            };
        }
    }
}