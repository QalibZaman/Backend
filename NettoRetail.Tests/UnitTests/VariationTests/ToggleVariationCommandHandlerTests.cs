using NettoRetail.Application.CQRS.Variations.Commands;
using NettoRetail.Application.Exceptions;
using NettoRetail.Domain.Entities;
using NettoRetail.Domain.Enums;
using NettoRetail.Shared.Interfaces.Repositories;
using NSubstitute;

namespace NettoRetail.Tests.UnitTests.VariationTests;

public class ToggleVariationCommandHandlerTests
{
    [Fact]
    public async Task ToggleExists_ShouldThrow_WhenItemIsNotVariation()
    {
        var repository = Substitute.For<IInventoryItemRepository>();
        var handler = new ToggleExistsCommandHandler(repository);

        repository.GetAsync(Arg.Any<NettoRetail.DataAccess.Specifications.Inventory.GetInventoryItemByIdAndShopIdSpecification>(), Arg.Any<CancellationToken>())
            .Returns(CreateItem(isVariation: false));

        await Assert.ThrowsAsync<BadRequestException>(() =>
            handler.Handle(new ToggleExistsCommand(Guid.NewGuid(), 7), CancellationToken.None));
    }

    [Fact]
    public async Task ToggleIsActive_ShouldThrow_WhenItemIsNotVariation()
    {
        var repository = Substitute.For<IInventoryItemRepository>();
        var handler = new ToggleIsActiveStatusCommandHandler(repository);

        repository.GetAsync(Arg.Any<NettoRetail.DataAccess.Specifications.Inventory.GetInventoryItemByIdAndShopIdSpecification>(), Arg.Any<CancellationToken>())
            .Returns(CreateItem(isVariation: false));

        await Assert.ThrowsAsync<BadRequestException>(() =>
            handler.Handle(new ToggleIsActiveStatusCommand(Guid.NewGuid(), 7), CancellationToken.None));
    }

    private static InventoryItemEntity CreateItem(bool isVariation)
    {
        return new InventoryItemEntity
        {
            Id = Guid.NewGuid(),
            ProductName = "Item",
            ProductUpc = "123456789012",
            Price = 10m,
            Description = "Description",
            CategoryId = 1,
            LastUpdatedDate = DateTime.UtcNow,
            IsActive = true,
            Status = InventoryItemStatusEnum.Available,
            AvailableAmount = 1,
            ReservedAmount = 0,
            OnHandAmount = 1,
            ProductLocation = "Shelf A",
            ShopId = 7,
            HasVariations = false,
            IsVariation = isVariation,
            Exists = true,
            Category = null!,
            BrandId = 1,
            Brand = null!,
            Images = [],
            OrderItems = null!,
            ComparisonItems = null!
        };
    }
}
