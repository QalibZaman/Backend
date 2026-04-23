using AutoFixture;
using MapsterMapper;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using NettoRetail.Application.Interfaces;
using NettoRetail.Application.Models.Request.Variation;
using NettoRetail.Application.Services.IInventoryService;
using NettoRetail.Domain.Entities;
using NettoRetail.Domain.Enums;
using NettoRetail.Shared.Interfaces.Repositories;
using NettoRetail.Shared.Options;
using NettoRetail.Tests.Helpers;
using NSubstitute;

namespace NettoRetail.Tests.UnitTests.ShopTests;

public class InventoryServiceTests
{
    private readonly Fixture _fixture;
    private readonly IInventoryService _inventoryService;
    private readonly IMapper _mapper;

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
        _mapper = MapperFactory.Create();

        _fixture = new Fixture();

        _inventoryService = new InventoryService(
            options,
            inventoryRepo,
            csvService,
            categoryRepo,
            brandRepo,
            logger,
            _mapper
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

    [Fact]
    public void BuildVariationInventoryItemEntities_ShouldApplyDefaultVariationState()
    {
        // Arrange
        var parent = new InventoryItemEntity
        {
            Id = Guid.NewGuid(),
            ProductName = "Parent",
            ProductUpc = "999999999999",
            Price = 199.99m,
            Description = "Parent description",
            CategoryId = 2,
            LastUpdatedDate = DateTime.UtcNow,
            IsActive = true,
            Status = InventoryItemStatusEnum.Available,
            AvailableAmount = 5,
            ReservedAmount = 1,
            OnHandAmount = 6,
            ProductLocation = "Warehouse",
            ShopId = 4,
            HasVariations = false,
            IsVariation = false,
            Exists = true,
            Category = null!,
            BrandId = 3,
            Brand = null!,
            Images = [],
            OrderItems = [],
            ComparisonItems = []
        };

        var models = new List<VariationModel>
        {
            new()
            {
                ProductName = "  White 128 GB  ",
                ProductUpc = " 123456789012 ",
                IsActive = false
            }
        };

        // Act
        var created = _inventoryService.BuildVariationInventoryItemEntities(models, parent);

        // Assert
        var variation = Assert.Single(created);
        Assert.Equal("White 128 GB", variation.ProductName);
        Assert.Equal("123456789012", variation.ProductUpc);
        Assert.Equal(parent.Price, variation.Price);
        Assert.Equal(parent.Description, variation.Description);
        Assert.Equal(parent.CategoryId, variation.CategoryId);
        Assert.Equal(parent.BrandId, variation.BrandId);
        Assert.Equal(parent.ShopId, variation.ShopId);
        Assert.Equal(parent.Id, variation.ParentItemId);
        Assert.True(variation.IsVariation);
        Assert.True(variation.IsActive);
        Assert.True(variation.Exists);
        Assert.Equal(InventoryItemStatusEnum.Available, variation.Status);
        Assert.Equal(0, variation.AvailableAmount);
        Assert.Equal(0, variation.ReservedAmount);
        Assert.Equal(0, variation.OnHandAmount);
    }
}
