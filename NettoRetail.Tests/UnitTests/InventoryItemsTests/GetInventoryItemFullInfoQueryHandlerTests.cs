using NettoRetail.Application.CQRS.InventoryItems.Queries;
using NettoRetail.Application.Exceptions;
using NettoRetail.DataAccess.Specifications.Inventory;
using NettoRetail.Domain.Entities;
using NettoRetail.Domain.Enums;
using NettoRetail.Shared.Interfaces.Repositories;
using NSubstitute;

namespace NettoRetail.Tests.UnitTests.InventoryItemsTests;

public class GetInventoryItemFullInfoQueryHandlerTests
{
    private readonly IInventoryItemRepository _inventoryRepo = Substitute.For<IInventoryItemRepository>();
    private readonly ICategoryRepository _categoryRepo = Substitute.For<ICategoryRepository>();

    private static InventoryItemEntity MakeItem(Guid id)
    {
        var brand = new BrandEntity
        {
            Id = 5,
            ShopId = 1,
            Name = "CoolBrand"
        };

        var category = new CategoryEntity
        {
            Id = 10,
            Name = "Soft Drinks",
            Level = 2,
            ShopId = 1,
            URL = "soft-drinks",
            Visibility = true
        };

        return new InventoryItemEntity
        {
            Id = id,
            ShopId = 1,
            ProductName = "Cola 330",
            ProductUpc = 123456789012,
            Description = "Canned drink",
            Price = 1.99m,
            PromotionPrice = null,
            AvailableAmount = 12,
            ReservedAmount = 0,
            OnHandAmount = 12,
            LastUpdatedDate = DateTime.UtcNow,
            IsActive = true,
            Status = InventoryItemStatusEnum.Available,
            ProductLocation = "A1",
            CategoryId = category.Id,
            Category = category,
            BrandId = brand.Id,
            Brand = brand,
            Images = new List<InventoryItemImageEntity>
            {
                new() { Order = 1, BlobUrl = "https://cdn/img-1.jpg" }
            }
        };
    }

    private static List<CategoryEntity> Breadcrumbs() => new()
    {
        new CategoryEntity { Id = 1,  Name = "Groceries",  Level = 0, ShopId = 1, URL = "groceries",  Visibility = true },
        new CategoryEntity { Id = 10, Name = "Soft Drinks", Level = 2, ShopId = 1, URL = "soft-drinks", Visibility = true }
    };

    [Fact]
    public async Task Handle_ShouldReturnMappedInfo_WithBreadcrumbs()
    {
        // Arrange
        var id = Guid.NewGuid();
        var item = MakeItem(id);

        _inventoryRepo.GetAsync(Arg.Any<GetItemByIdSpecification>(), Arg.Any<CancellationToken>())
                      .Returns(item);

        _categoryRepo.GetCategoryWithHierarchyAsync(item.CategoryId, Arg.Any<CancellationToken>())
                     .Returns(Task.FromResult(Breadcrumbs()));

        var handler = new GetInventoryItemFullInfoQueryHandler(_inventoryRepo, _categoryRepo);

        // Act
        var result = await handler.Handle(new GetInventoryItemFullInfoQuery(id), CancellationToken.None);

        Assert.NotNull(result);
        Assert.Equal(item.ProductName, result.ProductName);
        Assert.Equal(item.Price, result.Price);
        Assert.Equal(item.Status.ToString(), result.Status);

        Assert.NotNull(result.ProductCategories);
        Assert.Equal(2, result.ProductCategories.Count());
        Assert.Contains(result.ProductCategories, c => c.Name == "Groceries");
        Assert.Contains(result.ProductCategories, c => c.Name == "Soft Drinks");
    }

    [Fact]
    public async Task Handle_ShouldThrowNotFound_WhenItemMissing()
    {
        // Arrange
        _inventoryRepo.GetAsync(Arg.Any<GetItemByIdSpecification>(), Arg.Any<CancellationToken>())
                      .Returns((InventoryItemEntity?)null);

        var handler = new GetInventoryItemFullInfoQueryHandler(_inventoryRepo, _categoryRepo);

        // Act
        var act = () => handler.Handle(new GetInventoryItemFullInfoQuery(Guid.NewGuid()), CancellationToken.None);

        // Assert
        await Assert.ThrowsAsync<NotFoundException>(act);
        await _categoryRepo.DidNotReceive().GetCategoryWithHierarchyAsync(Arg.Any<int>(), Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task Handle_ShouldPassCancellationToken_ToRepositories()
    {
        // Arrange
        var id = Guid.NewGuid();
        var token = new CancellationTokenSource().Token;
        var item = MakeItem(id);

        _inventoryRepo.GetAsync(Arg.Any<GetItemByIdSpecification>(), token)
                      .Returns(item);

        _categoryRepo.GetCategoryWithHierarchyAsync(item.CategoryId, token)
                     .Returns(Task.FromResult(Breadcrumbs()));

        var handler = new GetInventoryItemFullInfoQueryHandler(_inventoryRepo, _categoryRepo);

        // Act
        _ = await handler.Handle(new GetInventoryItemFullInfoQuery(id), token);

        // Assert
        await _inventoryRepo.Received(1).GetAsync(Arg.Any<GetItemByIdSpecification>(), token);
        await _categoryRepo.Received(1).GetCategoryWithHierarchyAsync(item.CategoryId, token);
    }
}