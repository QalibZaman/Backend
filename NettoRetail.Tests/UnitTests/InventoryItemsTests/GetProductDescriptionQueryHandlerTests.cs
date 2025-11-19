using NettoRetail.Application.CQRS.InventoryItems.Queries;
using NettoRetail.Application.Exceptions;
using NettoRetail.DataAccess.Specifications.Inventory;
using NettoRetail.Domain.Entities;
using NettoRetail.Domain.Enums;
using NettoRetail.Shared.Interfaces.Repositories;
using NSubstitute;

namespace NettoRetail.Tests.UnitTests.InventoryItems;
public class GetInventoryItemDescriptionQueryHandlerTests
{
    private readonly IInventoryItemRepository _inventoryRepo;
    private readonly GetInventoryItemDescriptionQueryHandler _handler;

    public GetInventoryItemDescriptionQueryHandlerTests()
    {
        _inventoryRepo = Substitute.For<IInventoryItemRepository>();
        _handler = new GetInventoryItemDescriptionQueryHandler(_inventoryRepo);
    }

    [Fact]
    public async Task Handle_ShouldReturnDescription_WhenItemExists()
    {
        // Arrange
        var itemId = Guid.NewGuid();
        var expectedDescription = "<p>Sample description</p>";
        var entity = CreateTestEntity(itemId, expectedDescription);

        _inventoryRepo.GetAsync(
            Arg.Is<GetInventoryItemByIdSpecification>(s => s.Criteria.Compile().Invoke(entity)),
            Arg.Any<CancellationToken>())
            .Returns(entity);

        var query = new GetInventoryItemDescriptionQuery(itemId);

        // Act
        var result = await _handler.Handle(query, CancellationToken.None);

        // Assert
        Assert.Equal(expectedDescription, result);
    }


    [Fact]
    public async Task Handle_ShouldReturnEmptyString_WhenDescriptionIsNull()
    {
        // Arrange
        var itemId = Guid.NewGuid();
        var entity = CreateTestEntity(itemId, null);

        _inventoryRepo.GetAsync(Arg.Any<GetInventoryItemByIdSpecification>(), Arg.Any<CancellationToken>())
            .Returns(entity);

        var query = new GetInventoryItemDescriptionQuery(itemId);

        // Act
        var result = await _handler.Handle(query, CancellationToken.None);

        // Assert
        Assert.Equal(string.Empty, result);
    }

    [Fact]
    public async Task Handle_ShouldThrowNotFoundException_WhenItemDoesNotExist()
    {
        // Arrange
        _inventoryRepo.GetAsync(Arg.Any<GetInventoryItemByIdSpecification>(), Arg.Any<CancellationToken>())
            .Returns((InventoryItemEntity)null!);

        var query = new GetInventoryItemDescriptionQuery(Guid.NewGuid());

        // Act & Assert
        await Assert.ThrowsAsync<NotFoundException>(() =>
            _handler.Handle(query, CancellationToken.None));
    }

    [Fact]
    public async Task Handle_ShouldThrowNotFoundException_WhenItemNotFound()
    {
        // Arrange
        _inventoryRepo.GetAsync(
            Arg.Any<GetInventoryItemByIdSpecification>(),
            Arg.Any<CancellationToken>())
            .Returns((InventoryItemEntity)null!);

        var query = new GetInventoryItemDescriptionQuery(Guid.NewGuid());

        // Act & Assert
        await Assert.ThrowsAsync<NotFoundException>(() =>
            _handler.Handle(query, CancellationToken.None));
    }


    [Fact]
    public async Task Handle_ShouldReturnWhitespace_WhenDescriptionContainsOnlySpaces()
    {
        // Arrange
        var itemId = Guid.NewGuid();
        var entity = CreateTestEntity(itemId, "   "); 

        _inventoryRepo.GetAsync(Arg.Any<GetInventoryItemByIdSpecification>(), Arg.Any<CancellationToken>())
            .Returns(entity);

        // Act
        var result = await _handler.Handle(new GetInventoryItemDescriptionQuery(itemId), CancellationToken.None);

        // Assert
        Assert.Equal("   ", result); 
    }

    [Fact]
    public async Task Handle_ShouldReturnHtml_WhenDescriptionContainsTags()
    {
        // Arrange
        var itemId = Guid.NewGuid();
        var html = "<p><strong>New product</strong> — best quality!</p>";
        var entity = CreateTestEntity(itemId, html);

        _inventoryRepo.GetAsync(Arg.Any<GetInventoryItemByIdSpecification>(), Arg.Any<CancellationToken>())
            .Returns(entity);

        // Act
        var result = await _handler.Handle(new GetInventoryItemDescriptionQuery(itemId), CancellationToken.None);

        // Assert
        Assert.Equal(html, result);
        Assert.Contains("<strong>", result); 
    }

    [Fact]
    public async Task Handle_ShouldCallRepositoryOnlyOnce()
    {
        // Arrange
        var itemId = Guid.NewGuid();
        var entity = CreateTestEntity(itemId, "Description");

        _inventoryRepo.GetAsync(Arg.Any<GetInventoryItemByIdSpecification>(), Arg.Any<CancellationToken>())
            .Returns(entity);

        var query = new GetInventoryItemDescriptionQuery(itemId);

        // Act
        _ = await _handler.Handle(query, CancellationToken.None);

        // Assert
        await _inventoryRepo.Received(1)
            .GetAsync(Arg.Any<GetInventoryItemByIdSpecification>(), Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task Handle_ShouldThrowOperationCanceled_WhenCancelled()
    {
        // Arrange
        var itemId = Guid.NewGuid();
        var entity = CreateTestEntity(itemId, "desc");

        _inventoryRepo.GetAsync(Arg.Any<GetInventoryItemByIdSpecification>(), Arg.Any<CancellationToken>())
            .Returns(async ci =>
            {
                var ct = ci.ArgAt<CancellationToken>(1);
                await Task.Delay(1000, ct); 
                return entity;
            });

        using var cts = new CancellationTokenSource();
        cts.CancelAfter(10); 

        var query = new GetInventoryItemDescriptionQuery(itemId);

        // Act & Assert
        await Assert.ThrowsAnyAsync<OperationCanceledException>(() =>
            _handler.Handle(query, cts.Token));
    }



    private static InventoryItemEntity CreateTestEntity(Guid id, string? description = "<p>Default</p>")
    {
        return new InventoryItemEntity
        {
            Id = id,
            ProductName = "Test Product",
            ProductUpc = 123456789012,
            Price = 10.99m,
            Description = description ?? string.Empty,
            CategoryId = 1,
            LastUpdatedDate = DateTime.UtcNow,
            IsActive = true,
            Status = InventoryItemStatusEnum.Available,
            AvailableAmount = 10,
            ReservedAmount = 0,
            OnHandAmount = 10,
            ProductLocation = "Test Location",
            ShopId = 1,
            Category = new CategoryEntity
            {
                Name = "Category A",
                URL = "category-a",
                Visibility = true,
                ShopId = 1
            },
            BrandId = 1,
            Brand = new BrandEntity
            {
                Id = 1,
                Name = "Brand A",
                ShopId = 1
            }
        };
    }
}
