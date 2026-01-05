using AutoFixture;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using NettoRetail.Application.Interfaces;
using NettoRetail.Application.Services.IInventoryService;
using NettoRetail.Domain.Entities;
using NettoRetail.Shared.Interfaces.Repositories;
using NettoRetail.Shared.Options;
using NSubstitute;

namespace NettoRetail.Tests.UnitTests.ShopTests;

public class InventoryServiceTests
{
    private readonly Fixture _fixture;
    private readonly IInventoryService _inventoryService;

    public InventoryServiceTests()
    {

        var options = Options.Create(new InventoryItemOptions
        {
            NewItemDaysThreshold = 7
        });

        var inventoryRepo = Substitute.For<IInventoryItemRepository>();
        var csvService = Substitute.For<ICsvFileService>();
        var categoryRepo = Substitute.For<ICategoryRepository>();
        var brandRepo = Substitute.For<IBrandRepository>();
        var logger = Substitute.For<ILogger<InventoryService>>();

        _fixture = new Fixture();

        _inventoryService = new InventoryService(
            options,
            inventoryRepo,
            csvService,
            categoryRepo,
            brandRepo,
            logger
        );
    }

    [Fact]
    public void IsItemNew_ShouldReturnTrue_WhenItemLastUpdatedWithinThreshold()
    {
        // Arrange
        var item = _fixture.Build<InventoryItemEntity>()
            .With(x => x.LastUpdatedDate, DateTime.UtcNow.AddDays(-3))
            .OmitAutoProperties()
            .Create();

        // Act
        var result = _inventoryService.IsItemNew(item);

        // Assert
        Assert.True(result);
    }

    [Fact]
    public void IsItemNew_ShouldReturnFalse_WhenItemLastUpdatedMoreThanThreshold()
    {
        // Arrange
        var item = _fixture.Build<InventoryItemEntity>()
            .With(x => x.LastUpdatedDate, DateTime.UtcNow.AddDays(-8))
            .OmitAutoProperties()
            .Create();

        // Act
        var result = _inventoryService.IsItemNew(item);

        // Assert
        Assert.False(result);
    }

    [Fact]
    public void IsItemOutOfStock_ShouldReturnTrue_WhenItemAvailableAmountIsZero()
    {
        // Arrange
        var item = _fixture.Build<InventoryItemEntity>()
            .With(x => x.AvailableAmount, 0)
            .OmitAutoProperties()
            .Create();

        // Act
        var result = _inventoryService.IsItemOutOfStock(item);

        // Assert
        Assert.True(result);
    }

    [Fact]
    public void IsItemOutOfStock_ShouldReturnTrue_WhenItemAvailableAmountIsLowerZero()
    {
        // Arrange
        var item = _fixture.Build<InventoryItemEntity>()
            .With(x => x.AvailableAmount, -5)
            .OmitAutoProperties()
            .Create();

        // Act
        var result = _inventoryService.IsItemOutOfStock(item);

        // Assert
        Assert.True(result);
    }

    [Fact]
    public void IsItemOutOfStock_ShouldReturnFalse_WhenItemAvailableAmountGreaterThanZero()
    {
        // Arrange
        var item = _fixture.Build<InventoryItemEntity>()
            .With(x => x.AvailableAmount, 5)
            .OmitAutoProperties()
            .Create();

        // Act
        var result = _inventoryService.IsItemOutOfStock(item);

        // Assert
        Assert.False(result);
    }

    [Fact]
    public void IsItemOnSale_ShouldReturnTrue_WhenItemPromotionPriceIsNotNull()
    {
        // Arrange
        var item = _fixture.Build<InventoryItemEntity>()
            .With(x => x.Price, 500m)
            .With(x => x.PromotionPrice, 400m)
            .OmitAutoProperties()
            .Create();

        // Act
        var result = _inventoryService.IsItemOnSale(item);

        // Assert
        Assert.True(result);
    }

    [Fact]
    public void IsItemOnSale_ShouldReturnFalse_WhenItemPromotionPriceIsNull()
    {
        // Arrange
        var item = _fixture.Build<InventoryItemEntity>()
            .With(x => x.Price, 500m)
            .OmitAutoProperties()
            .Create();

        // Act
        var result = _inventoryService.IsItemOnSale(item);

        // Assert
        Assert.False(result);
    }
}
