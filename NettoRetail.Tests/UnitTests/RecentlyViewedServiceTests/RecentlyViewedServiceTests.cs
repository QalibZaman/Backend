using NettoRetail.Application.Models.Request.RecentlyViewedProduct;
using NettoRetail.Application.Services.IRecentlyViewedService;
using NettoRetail.Domain.Entities;
using NettoRetail.Shared.Interfaces.Repositories;
using NettoRetail.Shared.Specifications;
using NSubstitute;
using NSubstitute.ExceptionExtensions;
using NSubstitute.ReturnsExtensions;

namespace NettoRetail.Tests.UnitTests.RecentlyViewedServiceTests;

public class RecentlyViewedServiceTests
{
    private readonly IRecentlyViewedRepository _repository;
    private readonly RecentlyViewedService _service;
    private readonly CancellationToken _ct = CancellationToken.None;

    public RecentlyViewedServiceTests()
    {
        _repository = Substitute.For<IRecentlyViewedRepository>();
        _service = new RecentlyViewedService(_repository);
    }

    [Fact]
    public async Task AddOrUpdateAsync_ShouldAddNew_WhenNoExistingItem()
    {
        // Arrange
        var accountId = Guid.NewGuid();
        var inventoryItemId = Guid.NewGuid();

        _repository.GetAsync(Arg.Any<Specification<RecentlyViewedProductEntity>>(), _ct)
            .ReturnsNull();


        // Act
        await _service.AddOrUpdateAsync(accountId, inventoryItemId, _ct);

        // Assert
        await _repository.Received(1)
            .AddAsync(Arg.Is<RecentlyViewedProductEntity>(x =>
                x.AccountId == accountId &&
                x.InventoryItemId == inventoryItemId),
                _ct);

        await _repository.Received(1)
            .GetAllBySpecAsync(Arg.Any<Specification<RecentlyViewedProductEntity>>(), _ct);
    }

    [Fact]
    public async Task AddOrUpdateAsync_ShouldUpdateTimestamp_WhenExistingItemFound()
    {
        // Arrange
        var accountId = Guid.NewGuid();
        var inventoryItemId = Guid.NewGuid();

        var existing = new RecentlyViewedProductEntity
        {
            AccountId = accountId,
            InventoryItemId = inventoryItemId,
            ViewedAtUtc = DateTime.UtcNow.AddDays(-2)
        };

        var originalTime = existing.ViewedAtUtc;

        _repository.GetAsync(Arg.Any<Specification<RecentlyViewedProductEntity>>(), _ct)
            .Returns(existing);

        // Act
        await _service.AddOrUpdateAsync(accountId, inventoryItemId, _ct);

        // Assert
        await _repository.Received(1)
            .UpdateAsync(Arg.Is<RecentlyViewedProductEntity>(x =>
                x.AccountId == accountId &&
                x.InventoryItemId == inventoryItemId &&
                x.ViewedAtUtc > originalTime),
                _ct);

        await _repository.Received(1)
            .GetAllBySpecAsync(Arg.Any<Specification<RecentlyViewedProductEntity>>(), _ct);
    }


    [Fact]
    public async Task AddOrUpdateAsync_ShouldAlwaysEnforceMaxLimit()
    {
        // Arrange
        var accountId = Guid.NewGuid();
        var inventoryItemId = Guid.NewGuid();

        _repository.GetAsync(Arg.Any<Specification<RecentlyViewedProductEntity>>(), _ct)
            .Returns((RecentlyViewedProductEntity?)null);

        // Act
        await _service.AddOrUpdateAsync(accountId, inventoryItemId, _ct);

        // Assert
        await _repository.Received(1)
            .GetAllBySpecAsync(Arg.Any<Specification<RecentlyViewedProductEntity>>(), _ct);
    }

    [Fact]
    public async Task AddOrUpdateAsync_ShouldThrow_WhenRepositoryThrowsException()
    {
        // Arrange
        var accountId = Guid.NewGuid();
        var inventoryItemId = Guid.NewGuid();

        _repository
            .GetAsync(Arg.Any<Specification<RecentlyViewedProductEntity>>(), Arg.Any<CancellationToken>())
            .ThrowsAsync(new Exception("DB failure"));

        // Act & Assert
        var ex = await Assert.ThrowsAsync<Exception>(
            () => _service.AddOrUpdateAsync(accountId, inventoryItemId, CancellationToken.None));

        Assert.Equal("DB failure", ex.Message);
    }

    [Fact]
    public async Task AddOrUpdateAsync_ShouldHandleNullEntityFromRepositoryGracefully()
    {
        // Arrange
        var accountId = Guid.NewGuid();
        var inventoryItemId = Guid.NewGuid();

        // Simulate repository returns null, but no exception
        _repository.GetAsync(Arg.Any<Specification<RecentlyViewedProductEntity>>(), Arg.Any<CancellationToken>())
            .Returns((RecentlyViewedProductEntity?)null);

        // Act
        var ex = await Record.ExceptionAsync(() =>
            _service.AddOrUpdateAsync(accountId, inventoryItemId, CancellationToken.None));

        // Assert
        Assert.Null(ex);
        await _repository.Received(1).AddAsync(Arg.Any<RecentlyViewedProductEntity>(), Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task AddOrUpdateAsync_ShouldNotFail_IfUpdateReturnsNull()
    {
        // Arrange
        var accountId = Guid.NewGuid();
        var inventoryItemId = Guid.NewGuid();

        var existing = new RecentlyViewedProductEntity
        {
            AccountId = accountId,
            InventoryItemId = inventoryItemId,
            ViewedAtUtc = DateTime.UtcNow.AddDays(-3)
        };

        _repository.GetAsync(Arg.Any<Specification<RecentlyViewedProductEntity>>(), Arg.Any<CancellationToken>())
            .Returns(existing);

        _repository.UpdateAsync(Arg.Any<RecentlyViewedProductEntity>(), Arg.Any<CancellationToken>())
            .Returns((RecentlyViewedProductEntity?)null);

        // Act
        var ex = await Record.ExceptionAsync(() =>
            _service.AddOrUpdateAsync(accountId, inventoryItemId, CancellationToken.None));

        // Assert
        Assert.Null(ex);
        await _repository.Received(1).UpdateAsync(Arg.Any<RecentlyViewedProductEntity>(), Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task AddOrUpdateAsync_ShouldNotFail_IfAddReturnsNull()
    {
        // Arrange
        var accountId = Guid.NewGuid();
        var inventoryItemId = Guid.NewGuid();

        _repository.GetAsync(Arg.Any<Specification<RecentlyViewedProductEntity>>(), Arg.Any<CancellationToken>())
            .Returns((RecentlyViewedProductEntity?)null);

        _repository.AddAsync(Arg.Any<RecentlyViewedProductEntity>(), Arg.Any<CancellationToken>())
            .Returns((RecentlyViewedProductEntity?)null);

        // Act
        var ex = await Record.ExceptionAsync(() =>
            _service.AddOrUpdateAsync(accountId, inventoryItemId, CancellationToken.None));

        // Assert
        Assert.Null(ex);
        await _repository.Received(1).AddAsync(Arg.Any<RecentlyViewedProductEntity>(), Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task AddOrUpdateAsync_ShouldDeleteOldItems_WhenMoreThanFiveExist()
    {
        // Arrange
        var accountId = Guid.NewGuid();
        var inventoryItemId = Guid.NewGuid();

        _repository.GetAsync(Arg.Any<Specification<RecentlyViewedProductEntity>>(), _ct)
            .Returns((RecentlyViewedProductEntity?)null);

        var items = Enumerable.Range(0, 7)
            .Select(i => new RecentlyViewedProductEntity
            {
                AccountId = accountId,
                InventoryItemId = Guid.NewGuid(),
                ViewedAtUtc = DateTime.UtcNow.AddMinutes(-i)
            }).ToList();

        _repository.GetAllBySpecAsync(Arg.Any<Specification<RecentlyViewedProductEntity>>(), _ct)
            .Returns(items);

        // Act
        await _service.AddOrUpdateAsync(accountId, inventoryItemId, _ct);

        // Assert
        await _repository.Received(1)
            .DeleteManyBySpecAsync(Arg.Any<Specification<RecentlyViewedProductEntity>>(), _ct);
    }

    [Fact]
    public async Task AddOrUpdateAsync_ShouldNotDelete_WhenFiveOrFewerExist()
    {
        // Arrange
        var accountId = Guid.NewGuid();
        var inventoryItemId = Guid.NewGuid();

        _repository.GetAsync(Arg.Any<Specification<RecentlyViewedProductEntity>>(), _ct)
            .Returns((RecentlyViewedProductEntity?)null);

        var items = Enumerable.Range(0, 5)
            .Select(i => new RecentlyViewedProductEntity
            {
                AccountId = accountId,
                InventoryItemId = Guid.NewGuid(),
                ViewedAtUtc = DateTime.UtcNow.AddMinutes(-i)
            }).ToList();

        _repository.GetAllBySpecAsync(Arg.Any<Specification<RecentlyViewedProductEntity>>(), _ct)
            .Returns(items);

        // Act
        await _service.AddOrUpdateAsync(accountId, inventoryItemId, _ct);

        // Assert
        await _repository.DidNotReceive()
            .DeleteManyBySpecAsync(Arg.Any<Specification<RecentlyViewedProductEntity>>(), _ct);
    }

    [Fact]
    public async Task AddOrUpdateAsync_ShouldOnlyUpdate_WhenItemExists()
    {
        // Arrange
        var accountId = Guid.NewGuid();
        var inventoryItemId = Guid.NewGuid();

        var existing = new RecentlyViewedProductEntity
        {
            AccountId = accountId,
            InventoryItemId = inventoryItemId,
            ViewedAtUtc = DateTime.UtcNow.AddHours(-1)
        };

        _repository.GetAsync(Arg.Any<Specification<RecentlyViewedProductEntity>>(), _ct)
            .Returns(existing);

        // Act
        await _service.AddOrUpdateAsync(accountId, inventoryItemId, _ct);

        // Assert
        await _repository.Received(1).UpdateAsync(Arg.Any<RecentlyViewedProductEntity>(), _ct);
        await _repository.DidNotReceive().AddAsync(Arg.Any<RecentlyViewedProductEntity>(), _ct);
    }

    [Fact]
    public async Task MergeGuestItemsAsync_ShouldHandleNullList_Gracefully()
    {
        // Arrange
        var accountId = Guid.NewGuid();

        // Act
        var ex = await Record.ExceptionAsync(() =>
            _service.MergeGuestItemsAsync(accountId, null!, _ct));

        // Assert
        Assert.Null(ex);
        await _repository.Received(1)
            .MergeAsync(accountId, Arg.Is<IEnumerable<RecentlyViewedProductEntity>>(x => !x.Any()), _ct);
    }

    [Fact]
    public async Task MergeGuestItemsAsync_ShouldThrow_WhenRepositoryFails()
    {
        // Arrange
        var accountId = Guid.NewGuid();
        var guestItems = new List<MergeRecentlyViewedItem>
            {
                new() { InventoryItemId = Guid.NewGuid(), ViewedAtUtc = DateTime.UtcNow }
            };

        _repository.When(x => x.MergeAsync(accountId, Arg.Any<IEnumerable<RecentlyViewedProductEntity>>(), _ct))
            .Throw(new Exception("DB merge error"));

        // Act & Assert
        var ex = await Assert.ThrowsAsync<Exception>(
            () => _service.MergeGuestItemsAsync(accountId, guestItems, _ct));

        Assert.Equal("DB merge error", ex.Message);
    }

    [Fact]
    public async Task GetListAsync_ShouldReturnItemsOrderedByViewedAt()
    {
        // Arrange
        var accountId = Guid.NewGuid();
        var now = DateTime.UtcNow;

        var list = new List<RecentlyViewedProductEntity>
{
            new()
            {
                AccountId = accountId,
                ViewedAtUtc = now.AddMinutes(-5),
                InventoryItem = new InventoryItemEntity
                {
                    Id = Guid.NewGuid(),
                    ProductName = "Old",
                    ProductUpc = "123456",
                    Price = 10,
                    Description = "Test",
                    CategoryId = 1,
                    Category = null,
                    BrandId = 1,
                    Brand = null,
                    LastUpdatedDate = DateTime.UtcNow,
                    IsActive = true,
                    Status = NettoRetail.Domain.Enums.InventoryItemStatusEnum.Available,
                    AvailableAmount = 1,
                    ReservedAmount = 0,
                    OnHandAmount = 1,
                    ProductLocation = "loc",
                    ShopId = 1
                }
            },
            new()
            {
                AccountId = accountId,
                ViewedAtUtc = now,
                InventoryItem = new InventoryItemEntity
                {
                    Id = Guid.NewGuid(),
                    ProductName = "New",
                    ProductUpc = "654321",
                    Price = 15,
                    Description = "Test",
                    CategoryId = 1,
                    Category = null,
                    BrandId = 1,
                    Brand = null,
                    LastUpdatedDate = DateTime.UtcNow,
                    IsActive = true,
                    Status = NettoRetail.Domain.Enums.InventoryItemStatusEnum.Available,
                    AvailableAmount = 1,
                    ReservedAmount = 0,
                    OnHandAmount = 1,
                    ProductLocation = "loc",
                    ShopId = 1
                }
            }
        };


        _repository.GetAllBySpecAsync(Arg.Any<Specification<RecentlyViewedProductEntity>>(), _ct)
            .Returns(list);

        // Act
        var result = await _service.GetListAsync(accountId, _ct);

        // Assert
        Assert.Equal(2, result.Count);
        Assert.Equal("New", result.First().ProductName);
    }

    [Fact]
    public async Task GetListAsync_ShouldReturnEmptyList_WhenNoResults()
    {
        // Arrange
        _repository.GetAllBySpecAsync(Arg.Any<Specification<RecentlyViewedProductEntity>>(), _ct)
            .Returns(Enumerable.Empty<RecentlyViewedProductEntity>());

        // Act
        var result = await _service.GetListAsync(Guid.NewGuid(), _ct);

        // Assert
        Assert.Empty(result);
    }

    [Fact]
    public async Task MergeGuestItemsAsync_ShouldCallMergeAsync_WithCorrectItems()
    {
        // Arrange
        var accountId = Guid.NewGuid();
        var guestItems = new List<MergeRecentlyViewedItem>
        {
            new() { InventoryItemId = Guid.NewGuid(), ViewedAtUtc = DateTime.UtcNow },
            new() { InventoryItemId = Guid.NewGuid(), ViewedAtUtc = DateTime.UtcNow.AddMinutes(-10) }
        };

        // Act
        await _service.MergeGuestItemsAsync(accountId, guestItems, _ct);

        // Assert
        await _repository.Received(1).MergeAsync(
            accountId,
            Arg.Is<IEnumerable<RecentlyViewedProductEntity>>(x =>
                x.All(e => e.AccountId == accountId) &&
                x.Count() == guestItems.Count),
            _ct);
    }

    [Fact]
    public async Task MergeGuestItemsAsync_ShouldHandleEmptyGuestList()
    {
        // Arrange
        var accountId = Guid.NewGuid();
        var empty = Enumerable.Empty<MergeRecentlyViewedItem>();

        // Act
        var ex = await Record.ExceptionAsync(() =>
            _service.MergeGuestItemsAsync(accountId, empty, _ct));

        // Assert
        Assert.Null(ex);
        await _repository.Received(1)
            .MergeAsync(accountId, Arg.Is<IEnumerable<RecentlyViewedProductEntity>>(x => !x.Any()), _ct);
    }

    [Fact]
    public async Task FindExistingAsync_ShouldReturnEntity_WhenRepositoryFindsOne()
    {
        // Arrange
        var accountId = Guid.NewGuid();
        var itemId = Guid.NewGuid();
        var expected = new RecentlyViewedProductEntity { AccountId = accountId, InventoryItemId = itemId };

        _repository.GetAsync(Arg.Any<Specification<RecentlyViewedProductEntity>>(), _ct)
            .Returns(expected);

        // Use reflection to call the private method
        var method = typeof(RecentlyViewedService)
            .GetMethod("FindExistingAsync", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance)!;

        // Act
        var result = await (Task<RecentlyViewedProductEntity?>)method.Invoke(_service, new object[] { accountId, itemId, _ct })!;

        // Assert
        Assert.NotNull(result);
        Assert.Equal(expected, result);
    }

    [Fact]
    public async Task UpdateTimestampAsync_ShouldUpdateViewedAtUtc_AndCallRepositoryUpdate()
    {
        // Arrange
        var entity = new RecentlyViewedProductEntity
        {
            AccountId = Guid.NewGuid(),
            InventoryItemId = Guid.NewGuid(),
            ViewedAtUtc = DateTime.UtcNow.AddHours(-2)
        };

        var originalTime = entity.ViewedAtUtc;

        var method = typeof(RecentlyViewedService)
            .GetMethod("UpdateTimestampAsync", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance)!;

        // Act
        await (Task)method.Invoke(_service, new object[] { entity, _ct })!;

        // Assert
        await _repository.Received(1)
            .UpdateAsync(Arg.Is<RecentlyViewedProductEntity>(x => x.ViewedAtUtc > originalTime), _ct);
    }

    [Fact]
    public async Task InsertNewAsync_ShouldCreateEntity_AndCallAddAsync()
    {
        // Arrange
        var accountId = Guid.NewGuid();
        var itemId = Guid.NewGuid();

        var method = typeof(RecentlyViewedService)
            .GetMethod("InsertNewAsync", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance)!;

        // Act
        await (Task)method.Invoke(_service, new object[] { accountId, itemId, _ct })!;

        // Assert
        await _repository.Received(1)
            .AddAsync(Arg.Is<RecentlyViewedProductEntity>(x =>
                x.AccountId == accountId &&
                x.InventoryItemId == itemId &&
                x.ViewedAtUtc <= DateTime.UtcNow),
                _ct);
    }
}

