using AutoFixture;
using NettoRetail.Application.CQRS.Shops.Queries;
using NettoRetail.Application.Exceptions;
using NettoRetail.DataAccess.Specifications.Shop;
using NettoRetail.Domain.Entities;
using NettoRetail.Shared.Interfaces.Repositories;
using NettoRetail.Shared.Specifications;
using NSubstitute;
using NSubstitute.ReturnsExtensions;

namespace NettoRetail.Tests.UnitTests.ShopTests;

public class GetShopProductDetailsQueryHandlerTests
{
    private readonly IShopRepository _shopRepo;
    private readonly IInventoryItemRepository _itemsRepo;
    private readonly ICategoryRepository _categoryRepo;
    private readonly Fixture _fixture;

    public GetShopProductDetailsQueryHandlerTests()
    {
        _fixture = new Fixture();

        _shopRepo = Substitute.For<IShopRepository>();
        _itemsRepo = Substitute.For<IInventoryItemRepository>();
        _categoryRepo = Substitute.For<ICategoryRepository>();
    }

    [Fact]
    public async Task Handle_ShouldThrowNotFound_WhenShopDoesNotExist()
    {
        // Arrange
        var shopId = 1;
        var productId = Guid.NewGuid();

        var handler = new GetShopProductDetailsQueryHandler(_shopRepo, _itemsRepo, _categoryRepo);
        var query = new GetShopProductDetailsQuery(shopId, productId);

        _shopRepo.Exists(Arg.Any<GetShopByIdSpecification>(), Arg.Any<CancellationToken>())
            .Returns(false);

        // Act & Assert
        await Assert.ThrowsAsync<NotFoundException>(() => handler.Handle(query, CancellationToken.None));
    }

    [Fact]
    public async Task Handle_ShouldThrowNotFound_WhenItemDoesNotExist()
    {
        // Arrange
        var shopId = 1;
        var productId = Guid.NewGuid();

        var handler = new GetShopProductDetailsQueryHandler(_shopRepo, _itemsRepo, _categoryRepo);
        var query = new GetShopProductDetailsQuery(shopId, productId);

        _shopRepo.Exists(Arg.Any<GetShopByIdSpecification>(), Arg.Any<CancellationToken>())
            .Returns(true);

        _itemsRepo.GetAsync(Arg.Any<Specification<InventoryItemEntity>>(), Arg.Any<CancellationToken>())
            .ReturnsNull();

        // Act & Assert
        await Assert.ThrowsAsync<NotFoundException>(() => handler.Handle(query, CancellationToken.None));
    }

    [Fact]
    public async Task Handle_ShouldReturnValidResponse_WhenItemExist()
    {
        // Arrange
        var shopId = 1;
        var productId = Guid.NewGuid();

        var item = _fixture.Build<InventoryItemEntity>()
            .With(x => x.Id, productId)
            .With(x => x.ShopId, shopId)
            .With(x => x.ProductName, "Product")
            .With(x => x.Description, "Description")
            .With(x => x.Price, 150m)
            .With(x => x.AvailableAmount, 10)
            .With(x => x.LastUpdatedDate, DateTime.UtcNow)
            .With(x => x.CategoryId, 3)
            .OmitAutoProperties()
            .Create();

        item.Images = new List<InventoryItemImageEntity>
        {
            new InventoryItemImageEntity { BlobUrl = "url2", Order = 2 },
            new InventoryItemImageEntity { BlobUrl = "url1", Order = 1 }
        };

        var categories = new List<CategoryEntity>
        {
            new CategoryEntity { Id = 2, Name = "Category1", ShopId = shopId, URL = "category1", Visibility = true, Level = 1  },
            new CategoryEntity { Id = item.CategoryId, Name = "Category2", ShopId = shopId, URL = "category2", Visibility = true, ParentId = 2, Level = 2 }
        };

        var handler = new GetShopProductDetailsQueryHandler(_shopRepo, _itemsRepo, _categoryRepo);
        var query = new GetShopProductDetailsQuery(shopId, productId);

        _shopRepo.Exists(Arg.Any<GetShopByIdSpecification>(), Arg.Any<CancellationToken>())
            .Returns(true);

        _itemsRepo.GetAsync(Arg.Any<Specification<InventoryItemEntity>>(), Arg.Any<CancellationToken>())
            .Returns(item);

        _categoryRepo.GetCategoryWithHierarchyAsync(item.CategoryId, Arg.Any<CancellationToken>())
            .Returns(categories);

        // Act
        var result = await handler.Handle(query, CancellationToken.None);

        // Assert
        Assert.Equal(item.Id, result.Id);
        Assert.Equal(item.ProductName, result.ProductName);
        Assert.Equal(item.Description, result.Description);
        Assert.Equal(item.Price, result.Price);
        Assert.Equal(item.AvailableAmount, result.AvailableAmount);
        Assert.Equal(item.LastUpdatedDate, result.LastUpdatedDate);
        Assert.Null(result.PromotionPrice);
        Assert.Equal(["url1", "url2"], result.MediaUrls);
        Assert.Equal(["Category1", "Category2"], result.Categories);
    }

    [Fact]
    public async Task Handle_ShouldSetIsNewTrue_WhenItemLastUpdatedWithin7Days()
    {
        // Arrange
        var shopId = 1;
        var productId = Guid.NewGuid();

        var item = _fixture.Build<InventoryItemEntity>()
            .With(x => x.LastUpdatedDate, DateTime.UtcNow.AddDays(-3))
            .OmitAutoProperties()
            .Create();

        var categories = new List<CategoryEntity>
        {
            new CategoryEntity { Id = 1, Name = "Category", ShopId = shopId, URL = "category", Visibility = true, Level = 1  }
        };

        var handler = new GetShopProductDetailsQueryHandler(_shopRepo, _itemsRepo, _categoryRepo);
        var query = new GetShopProductDetailsQuery(shopId, productId);

        _shopRepo.Exists(Arg.Any<GetShopByIdSpecification>(), Arg.Any<CancellationToken>())
            .Returns(true);

        _itemsRepo.GetAsync(Arg.Any<Specification<InventoryItemEntity>>(), Arg.Any<CancellationToken>())
            .Returns(item);

        _categoryRepo.GetCategoryWithHierarchyAsync(item.CategoryId, Arg.Any<CancellationToken>())
            .Returns(categories);

        // Act
        var result = await handler.Handle(query, CancellationToken.None);

        // Assert
        Assert.True(result.IsNew);
    }

    [Fact]
    public async Task Handle_ShouldSetIsNewFalse_WhenItemLastUpdatedMoreThan7DaysAgo()
    {
        // Arrange
        var shopId = 1;
        var productId = Guid.NewGuid();

        var item = _fixture.Build<InventoryItemEntity>()
            .With(x => x.LastUpdatedDate, DateTime.UtcNow.AddDays(-8))
            .OmitAutoProperties()
            .Create();

        var categories = new List<CategoryEntity>
        {
            new CategoryEntity { Id = 1, Name = "Category", ShopId = shopId, URL = "category", Visibility = true, Level = 1  }
        };

        var handler = new GetShopProductDetailsQueryHandler(_shopRepo, _itemsRepo, _categoryRepo);
        var query = new GetShopProductDetailsQuery(shopId, productId);

        _shopRepo.Exists(Arg.Any<GetShopByIdSpecification>(), Arg.Any<CancellationToken>())
            .Returns(true);

        _itemsRepo.GetAsync(Arg.Any<Specification<InventoryItemEntity>>(), Arg.Any<CancellationToken>())
            .Returns(item);

        _categoryRepo.GetCategoryWithHierarchyAsync(item.CategoryId, Arg.Any<CancellationToken>())
            .Returns(categories);

        // Act
        var result = await handler.Handle(query, CancellationToken.None);

        // Assert
        Assert.False(result.IsNew);
    }

    [Fact]
    public async Task Handle_ShouldSetIsOutOfStockTrue_WhenItemAvailableAmountIsZero()
    {
        // Arrange
        var shopId = 1;
        var productId = Guid.NewGuid();

        var item = _fixture.Build<InventoryItemEntity>()
            .With(x => x.AvailableAmount, 0)
            .OmitAutoProperties()
            .Create();

        var categories = new List<CategoryEntity>
        {
            new CategoryEntity { Id = 1, Name = "Category", ShopId = shopId, URL = "category", Visibility = true, Level = 1  }
        };

        var handler = new GetShopProductDetailsQueryHandler(_shopRepo, _itemsRepo, _categoryRepo);
        var query = new GetShopProductDetailsQuery(shopId, productId);

        _shopRepo.Exists(Arg.Any<GetShopByIdSpecification>(), Arg.Any<CancellationToken>())
            .Returns(true);

        _itemsRepo.GetAsync(Arg.Any<Specification<InventoryItemEntity>>(), Arg.Any<CancellationToken>())
            .Returns(item);

        _categoryRepo.GetCategoryWithHierarchyAsync(item.CategoryId, Arg.Any<CancellationToken>())
            .Returns(categories);

        // Act
        var result = await handler.Handle(query, CancellationToken.None);

        // Assert
        Assert.True(result.IsOutOfStock);
    }

    [Fact]
    public async Task Handle_ShouldSetIsOutOfStockTrue_WhenItemAvailableAmountIsLowerZero()
    {
        // Arrange
        var shopId = 1;
        var productId = Guid.NewGuid();

        var item = _fixture.Build<InventoryItemEntity>()
            .With(x => x.AvailableAmount, -5)
            .OmitAutoProperties()
            .Create();

        var categories = new List<CategoryEntity>
        {
            new CategoryEntity { Id = 1, Name = "Category", ShopId = shopId, URL = "category", Visibility = true, Level = 1  }
        };

        var handler = new GetShopProductDetailsQueryHandler(_shopRepo, _itemsRepo, _categoryRepo);
        var query = new GetShopProductDetailsQuery(shopId, productId);

        _shopRepo.Exists(Arg.Any<GetShopByIdSpecification>(), Arg.Any<CancellationToken>())
            .Returns(true);

        _itemsRepo.GetAsync(Arg.Any<Specification<InventoryItemEntity>>(), Arg.Any<CancellationToken>())
            .Returns(item);

        _categoryRepo.GetCategoryWithHierarchyAsync(item.CategoryId, Arg.Any<CancellationToken>())
            .Returns(categories);

        // Act
        var result = await handler.Handle(query, CancellationToken.None);

        // Assert
        Assert.True(result.IsOutOfStock);
    }

    [Fact]
    public async Task Handle_ShouldSetIsOutOfStockFalse_WhenItemAvailableAmountGreaterThanZero()
    {
        // Arrange
        var shopId = 1;
        var productId = Guid.NewGuid();

        var item = _fixture.Build<InventoryItemEntity>()
            .With(x => x.AvailableAmount, 5)
            .OmitAutoProperties()
            .Create();

        var categories = new List<CategoryEntity>
        {
            new CategoryEntity { Id = 1, Name = "Category", ShopId = shopId, URL = "category", Visibility = true, Level = 1  }
        };

        var handler = new GetShopProductDetailsQueryHandler(_shopRepo, _itemsRepo, _categoryRepo);
        var query = new GetShopProductDetailsQuery(shopId, productId);

        _shopRepo.Exists(Arg.Any<GetShopByIdSpecification>(), Arg.Any<CancellationToken>())
            .Returns(true);

        _itemsRepo.GetAsync(Arg.Any<Specification<InventoryItemEntity>>(), Arg.Any<CancellationToken>())
            .Returns(item);

        _categoryRepo.GetCategoryWithHierarchyAsync(item.CategoryId, Arg.Any<CancellationToken>())
            .Returns(categories);

        // Act
        var result = await handler.Handle(query, CancellationToken.None);

        // Assert
        Assert.False(result.IsOutOfStock);
    }

    [Fact]
    public async Task Handle_ShouldSetIsOnSaleTrue_WhenItemPromotionPriceIsNotNull()
    {
        // Arrange
        var shopId = 1;
        var productId = Guid.NewGuid();

        var item = _fixture.Build<InventoryItemEntity>()
            .With(x => x.Price, 500m)
            .With(x => x.PromotionPrice, 400m)
            .OmitAutoProperties()
            .Create();

        var categories = new List<CategoryEntity>
        {
            new CategoryEntity { Id = 1, Name = "Category", ShopId = shopId, URL = "category", Visibility = true, Level = 1  }
        };

        var handler = new GetShopProductDetailsQueryHandler(_shopRepo, _itemsRepo, _categoryRepo);
        var query = new GetShopProductDetailsQuery(shopId, productId);

        _shopRepo.Exists(Arg.Any<GetShopByIdSpecification>(), Arg.Any<CancellationToken>())
            .Returns(true);

        _itemsRepo.GetAsync(Arg.Any<Specification<InventoryItemEntity>>(), Arg.Any<CancellationToken>())
            .Returns(item);

        _categoryRepo.GetCategoryWithHierarchyAsync(item.CategoryId, Arg.Any<CancellationToken>())
            .Returns(categories);

        // Act
        var result = await handler.Handle(query, CancellationToken.None);

        // Assert
        Assert.True(result.IsOnSale);
    }

    [Fact]
    public async Task Handle_ShouldSetIsOnSaleFalse_WhenItemPromotionPriceIsNull()
    {
        // Arrange
        var shopId = 1;
        var productId = Guid.NewGuid();

        var item = _fixture.Build<InventoryItemEntity>()
            .With(x => x.Price, 500m)
            .OmitAutoProperties()
            .Create();

        var categories = new List<CategoryEntity>
        {
            new CategoryEntity { Id = 1, Name = "Category", ShopId = shopId, URL = "category", Visibility = true, Level = 1  }
        };

        var handler = new GetShopProductDetailsQueryHandler(_shopRepo, _itemsRepo, _categoryRepo);
        var query = new GetShopProductDetailsQuery(shopId, productId);

        _shopRepo.Exists(Arg.Any<GetShopByIdSpecification>(), Arg.Any<CancellationToken>())
            .Returns(true);

        _itemsRepo.GetAsync(Arg.Any<Specification<InventoryItemEntity>>(), Arg.Any<CancellationToken>())
            .Returns(item);

        _categoryRepo.GetCategoryWithHierarchyAsync(item.CategoryId, Arg.Any<CancellationToken>())
            .Returns(categories);

        // Act
        var result = await handler.Handle(query, CancellationToken.None);

        // Assert
        Assert.False(result.IsOnSale);
    }
}
