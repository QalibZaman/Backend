using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using NettoRetail.Domain.Entities;
using NettoRetail.Domain.Entities.JunctionEntities;
using NettoRetail.Domain.Enums;
using NettoRetail.Shared.Interfaces.Repositories;
using NettoRetail.Shared.Specifications;
using System.Net;

namespace NettoRetail.Tests.IntegrationTests.OrderTests;

[Collection("Sequential Tests")]
public class GetOrderPrintingDetailsHandlerTests  : IClassFixture<CustomWebApplicationFactory>
{
    private readonly HttpClient _client;

    public GetOrderPrintingDetailsHandlerTests (CustomWebApplicationFactory factory)
    {
        var order = BuildTestOrder(9);

        var customizedFactory = factory.WithWebHostBuilder(builder =>
        {
            builder.ConfigureServices(services =>
            {
                services.RemoveAll(typeof(IOrderRepository));
                services.AddSingleton<IOrderRepository>(new FakeOrderRepository(order));
            });
        });

        _client = customizedFactory.CreateClient(new WebApplicationFactoryClientOptions
        {
            AllowAutoRedirect = false
        });
    }

    [Fact]
    public async Task PrintOrder_ShouldReturnPdfFile_WhenOrderExists()
    {
        // Act
        var response = await _client.GetAsync("api/v1/orders/print/9");

        // Assert
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);

        Assert.True(response.Content.Headers.ContentType != null);
        Assert.Equal("application/pdf", response.Content.Headers.ContentType.MediaType);

        Assert.True(response.Content.Headers.ContentDisposition != null || response.Headers.Contains("content-disposition"));

        var bytes = await response.Content.ReadAsByteArrayAsync();
        Assert.NotNull(bytes);
        Assert.True(bytes.Length > 0);
    }

    private static OrderEntity BuildTestOrder(int id)
    {
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

        var inventoryItem = new InventoryItemEntity
        {
            Id = Guid.NewGuid(),
            BrandId = 1,
            Brand = new BrandEntity { Id = 1, Name = "TestBrand", ShopId = 1 },
            ProductName = "Test Product",
            ProductUpc = 123456789000,
            Price = 10.5m,
            Description = "desc",
            CategoryId = 1,
            PromotionPrice = null,
            IsPromoted = false,
            LastUpdatedDate = DateTime.UtcNow,
            IsActive = true,
            Status = InventoryItemStatusEnum.Available,
            AvailableAmount = 10,
            ReservedAmount = 1,
            OnHandAmount = 11,
            ProductLocation = "loc",
            ShopId = 1,
            Category = new CategoryEntity { Id = 1, Name = "Cat", URL = "cat", Visibility = true, Level = 1, ShopId = 1 }
        };

        var orderItem = new OrderItemEntity
        {
            ItemId = inventoryItem.Id,
            InventoryItem = inventoryItem,
            Quantity = 2
        };

        var statusHistory = new List<OrderStatusHistoryEntity>
        {
            new OrderStatusHistoryEntity {
                OrderId = 9,
                Id = Guid.NewGuid(),
                UpdatedAt = DateTime.UtcNow,
                OldStatus = OrderStatusEnum.New,
                NewStatus = OrderStatusEnum.Processing,
                Comment = "",
                UpdatedBy = account
            }
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
            OrderItems = new List<OrderItemEntity> { orderItem },
            StatusHistory = statusHistory
        };

        orderItem.Order = order;

        return order;
    }

    private class FakeOrderRepository : IOrderRepository
    {
        private readonly OrderEntity _order;
        public FakeOrderRepository(OrderEntity order) => _order = order;

        public Task<OrderEntity?> GetOrderForPrinting(Specification<OrderEntity> specification, CancellationToken cancellationToken)
        {
            return Task.FromResult<OrderEntity?>(_order);
        }

        #region NotImplementedStubs
        public Task<OrderEntity> AddAsync(OrderEntity model, CancellationToken cancellationToken) => throw new NotImplementedException();
        public Task<OrderEntity?> GetAsync(Specification<OrderEntity> expression, CancellationToken cancellationToken) => throw new NotImplementedException();
        public Task<IEnumerable<OrderEntity>> GetManyBySpecAsync(Specification<OrderEntity> specification, CancellationToken cancellationToken) => throw new NotImplementedException();
        public Task<int> GetAmountAsync(CancellationToken cancellationToken) => throw new NotImplementedException();
        public Task DeleteAsync(Specification<OrderEntity> specification, CancellationToken cancellationToken) => throw new NotImplementedException();
        public Task DeleteAsync(OrderEntity entity, CancellationToken cancellationToken) => throw new NotImplementedException();
        public Task DeleteManyBySpecAsync(Specification<OrderEntity> specification, CancellationToken cancellationToken) => throw new NotImplementedException();
        public Task<OrderEntity> UpdateAsync(OrderEntity model, CancellationToken cancellationToken) => throw new NotImplementedException();
        public Task<IEnumerable<OrderEntity>> GetAllAsync(CancellationToken cancellationToken) => throw new NotImplementedException();
        public Task<IEnumerable<OrderEntity>> GetAllBySpecAsync(Specification<OrderEntity> specification, CancellationToken cancellationToken) => throw new NotImplementedException();
        public Task<bool> Exists(Specification<OrderEntity> specification, CancellationToken cancellationToken) => throw new NotImplementedException();
        public Task AddRangeAsync(IEnumerable<OrderEntity> entities, CancellationToken cancellationToken) => throw new NotImplementedException();
        public Task AddBulkAsync(IEnumerable<OrderEntity> entities, CancellationToken cancellationToken) => throw new NotImplementedException();
        public Task<int> GetAmountBySpecAsync(Specification<OrderEntity> specification, CancellationToken cancellationToken) => throw new NotImplementedException();
        public Task<(ICollection<OrderEntity> Items, int TotalCount)> GetAllPaginatedAsync(Specification<OrderEntity> specification, CancellationToken cancellationToken) => throw new NotImplementedException();
        public Task UpdateBulkAsync(IEnumerable<OrderEntity> entities, CancellationToken cancellationToken) => throw new NotImplementedException();
        public Task UpdateManyAsync(IEnumerable<OrderEntity> entities, CancellationToken cancellationToken) => throw new NotImplementedException();
        public Task<OrderEntity> GetWithRelatedDataAsync(Specification<OrderEntity> specification, CancellationToken cancellationToken) => throw new NotImplementedException();
        public Task<OrderEntity?> GetOrderWithInventoryItemsAsync(Specification<OrderEntity> specification, CancellationToken cancellationToken) => throw new NotImplementedException();
        #endregion
    }
}
