using NettoRetail.Application.CQRS.InventoryItems.Commands;
using NettoRetail.Application.Exceptions;
using NettoRetail.DataAccess.Specifications.Inventory;
using NettoRetail.Domain.Entities;
using NettoRetail.Domain.Enums;
using NettoRetail.Shared.Interfaces.Repositories;
using NSubstitute;

namespace NettoRetail.Tests.UnitTests.InventoryItems;
public class UpdateInventoryItemDescriptionCommandHandlerTests
{
    private readonly IInventoryItemRepository _inventoryRepo;
    private readonly UpdateInventoryItemDescriptionCommandHandler _handler;

    public UpdateInventoryItemDescriptionCommandHandlerTests()
    {
        _inventoryRepo = Substitute.For<IInventoryItemRepository>();
        _handler = new UpdateInventoryItemDescriptionCommandHandler(_inventoryRepo);
    }

    [Fact]
    public async Task Handle_ShouldUpdateDescription_WhenItemExists()
    {
        // Arrange
        var id = Guid.NewGuid();
        var entity = CreateTestEntity(id, "Old description");

        _inventoryRepo.GetAsync(Arg.Any<GetInventoryItemByIdSpecification>(), Arg.Any<CancellationToken>())
            .Returns(entity);

        var command = new UpdateInventoryItemDescriptionCommand(id, "New updated description");

        // Act
        await _handler.Handle(command, CancellationToken.None);

        // Assert
        await _inventoryRepo.Received(1)
            .UpdateAsync(Arg.Is<InventoryItemEntity>(x => x.Description == "New updated description"), Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task Handle_ShouldThrowNotFoundException_WhenItemNotFound()
    {
        // Arrange
        _inventoryRepo.GetAsync(Arg.Any<GetInventoryItemByIdSpecification>(), Arg.Any<CancellationToken>())
            .Returns((InventoryItemEntity)null!);

        var command = new UpdateInventoryItemDescriptionCommand(Guid.NewGuid(), "Some text");

        // Act & Assert
        await Assert.ThrowsAsync<NotFoundException>(() =>
            _handler.Handle(command, CancellationToken.None));
    }

    [Fact]
    public async Task Handle_ShouldCallUpdateOnce()
    {
        // Arrange
        var id = Guid.NewGuid();
        var entity = CreateTestEntity(id, "Initial desc");

        _inventoryRepo.GetAsync(Arg.Any<GetInventoryItemByIdSpecification>(), Arg.Any<CancellationToken>())
            .Returns(entity);

        var command = new UpdateInventoryItemDescriptionCommand(id, "Changed desc");

        // Act
        await _handler.Handle(command, CancellationToken.None);

        // Assert
        await _inventoryRepo.Received(1)
            .UpdateAsync(Arg.Any<InventoryItemEntity>(), Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task Handle_ShouldAllowEmptyDescription()
    {
        // Arrange
        var id = Guid.NewGuid();
        var entity = CreateTestEntity(id, "Old description");

        _inventoryRepo.GetAsync(Arg.Any<GetInventoryItemByIdSpecification>(), Arg.Any<CancellationToken>())
            .Returns(entity);

        var command = new UpdateInventoryItemDescriptionCommand(id, string.Empty);

        // Act
        await _handler.Handle(command, CancellationToken.None);

        // Assert
        await _inventoryRepo.Received(1)
            .UpdateAsync(Arg.Is<InventoryItemEntity>(x => x.Description == string.Empty), Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task Handle_ShouldThrowOperationCanceled_WhenCancelled()
    {
        // Arrange
        var id = Guid.NewGuid();
        var entity = CreateTestEntity(id, "desc");

        _inventoryRepo.GetAsync(Arg.Any<GetInventoryItemByIdSpecification>(), Arg.Any<CancellationToken>())
            .Returns(async ci =>
            {
                var ct = ci.ArgAt<CancellationToken>(1);
                await Task.Delay(500, ct);
                return entity;
            });

        using var cts = new CancellationTokenSource();
        cts.CancelAfter(10);

        var command = new UpdateInventoryItemDescriptionCommand(id, "new");

        // Act & Assert
        await Assert.ThrowsAnyAsync<OperationCanceledException>(() =>
            _handler.Handle(command, cts.Token));
    }

    [Fact]
    public async Task Handle_ShouldNotCallUpdate_WhenEntityIsNull()
    {
        // Arrange
        _inventoryRepo.GetAsync(Arg.Any<GetInventoryItemByIdSpecification>(), Arg.Any<CancellationToken>())
            .Returns((InventoryItemEntity)null!);

        var command = new UpdateInventoryItemDescriptionCommand(Guid.NewGuid(), "desc");

        // Act
        await Assert.ThrowsAsync<NotFoundException>(() =>
            _handler.Handle(command, CancellationToken.None));

        // Assert
        await _inventoryRepo.DidNotReceive().UpdateAsync(Arg.Any<InventoryItemEntity>(), Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task Handle_ShouldUpdateWithTrimmedDescription_WhenSpacesProvided()
    {
        // Arrange
        var id = Guid.NewGuid();
        var entity = CreateTestEntity(id, "old");

        _inventoryRepo.GetAsync(Arg.Any<GetInventoryItemByIdSpecification>(), Arg.Any<CancellationToken>())
            .Returns(entity);

        var command = new UpdateInventoryItemDescriptionCommand(id, "   New content   ");

        // Act
        await _handler.Handle(command, CancellationToken.None);

        // Assert
        await _inventoryRepo.Received(1)
         .UpdateAsync(
         Arg.Is<InventoryItemEntity>(x => x.Description == "   New content   "),
         Arg.Any<CancellationToken>());

    }

    [Fact]
    public async Task Handle_ShouldKeepOtherFieldsUnchanged_WhenUpdatingDescription()
    {
        // Arrange
        var id = Guid.NewGuid();
        var entity = CreateTestEntity(id, "Before");

        _inventoryRepo.GetAsync(Arg.Any<GetInventoryItemByIdSpecification>(), Arg.Any<CancellationToken>())
            .Returns(entity);

        var oldPrice = entity.Price;
        var oldStock = entity.AvailableAmount;

        var command = new UpdateInventoryItemDescriptionCommand(id, "After");

        // Act
        await _handler.Handle(command, CancellationToken.None);

        // Assert
        Assert.Equal(oldPrice, entity.Price);
        Assert.Equal(oldStock, entity.AvailableAmount);
    }

    [Fact]
    public async Task Handle_ShouldHandleVeryLongDescription_UpToLimit()
    {
        // Arrange
        var id = Guid.NewGuid();
        var entity = CreateTestEntity(id, "short");
        _inventoryRepo.GetAsync(Arg.Any<GetInventoryItemByIdSpecification>(), Arg.Any<CancellationToken>())
            .Returns(entity);

        string longDescription = new string('a', 5000); 
        var command = new UpdateInventoryItemDescriptionCommand(id, longDescription);

        // Act
        await _handler.Handle(command, CancellationToken.None);

        // Assert
        await _inventoryRepo.Received(1)
            .UpdateAsync(Arg.Is<InventoryItemEntity>(x => x.Description == longDescription), Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task Handle_ShouldNotFail_WhenDescriptionContainsHtmlTags()
    {
        // Arrange
        var id = Guid.NewGuid();
        var entity = CreateTestEntity(id, "old");
        _inventoryRepo.GetAsync(Arg.Any<GetInventoryItemByIdSpecification>(), Arg.Any<CancellationToken>())
            .Returns(entity);

        var html = "<h1>Title</h1><p>Details about product</p>";
        var command = new UpdateInventoryItemDescriptionCommand(id, html);

        // Act
        await _handler.Handle(command, CancellationToken.None);

        // Assert
        await _inventoryRepo.Received(1)
            .UpdateAsync(Arg.Is<InventoryItemEntity>(x => x.Description == html), Arg.Any<CancellationToken>());
    }

    private static InventoryItemEntity CreateTestEntity(Guid id, string description)
    {
        return new InventoryItemEntity
        {
            Id = id,
            ProductName = "Test Product",
            ProductUpc = 123456789012,
            Price = 10.99m,
            Description = description,
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
