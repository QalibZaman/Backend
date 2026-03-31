using System;
using System.Threading;
using System.Threading.Tasks;
using NettoRetail.Application.CQRS.Variations.Commands;
using NettoRetail.Domain.Entities;
using NettoRetail.Domain.Enums;
using NettoRetail.Shared.Interfaces.Repositories;
using NSubstitute;
using Xunit;

namespace NettoRetail.Tests.UnitTests.InventoryItemsTests;

public class ToggleExistsCommandHandlerTests
{
    private readonly IInventoryItemRepository _inventoryItemRepository;
    private readonly ToggleExistsCommandHandler _handler;

    public ToggleExistsCommandHandlerTests()
    {
        _inventoryItemRepository = Substitute.For<IInventoryItemRepository>();
        _handler = new ToggleExistsCommandHandler(_inventoryItemRepository);
    }

    [Fact]
    public async Task Handle_ShouldDeactivateVariation_WhenMovingToNonExistent()
    {
        var id = Guid.NewGuid();
        var shopId = 1;
        var variation = new InventoryItemEntity
        {
            Id = id,
            ProductName = "Test Variation",
            ProductUpc = "123456789012",
            Price = 10m,
            Description = "Test description",
            CategoryId = 1,
            LastUpdatedDate = DateTime.UtcNow,
            IsActive = true,
            Status = InventoryItemStatusEnum.Available,
            AvailableAmount = 1,
            ReservedAmount = 0,
            OnHandAmount = 1,
            ProductLocation = "Shelf A",
            ShopId = shopId,
            HasVariations = false,
            IsVariation = true,
            Exists = true,
            Category = null!,
            BrandId = 1,
            Brand = null!,
            Images = null!,
            OrderItems = null!,
            ComparisonItems = null!
        };

        _inventoryItemRepository.GetAsync(Arg.Any<NettoRetail.DataAccess.Specifications.Inventory.GetInventoryItemByIdAndShopIdSpecification>(), Arg.Any<CancellationToken>())
            .Returns(variation);

        var command = new ToggleExistsCommand(id, shopId);

        await _handler.Handle(command, CancellationToken.None);

        await _inventoryItemRepository.Received(1).UpdateAsync(
            Arg.Is<InventoryItemEntity>(x =>
                x.Id == id &&
                x.Exists == false &&
                x.IsActive == false &&
                x.Status == InventoryItemStatusEnum.Unavailable),
            Arg.Any<CancellationToken>());
    }
}
