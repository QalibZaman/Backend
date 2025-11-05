using NettoRetail.Application.CQRS.FavouriteListItems.Commands;
using NettoRetail.Application.Exceptions;
using NettoRetail.Application.Services.ICartService;
using NettoRetail.Application.Services.IFavouriteListItemsService;
using NettoRetail.Domain.Entities;
using NSubstitute;
using NSubstitute.ExceptionExtensions;

namespace NettoRetail.Tests.UnitTests.FavouriteListTests;

public class AddToCartFromFavouritesCommandHandlerTests
{
    private readonly ICartService _cartService;
    private readonly IFavouriteListItems _favouriteListItems;

    public AddToCartFromFavouritesCommandHandlerTests()
    {
        _cartService = Substitute.For<ICartService>();
        _favouriteListItems = Substitute.For<IFavouriteListItems>();
    }

    private static CartEntity CartWith(Guid customerId, Guid itemId, int qty) => new()
    {
        CustomerId = customerId,
        Items = new List<CartItemEntity>
        {
            new()
            {
                InventoryItemId = itemId,
                Quantity = qty,
                Price = 100
            }
        }
    };

    [Fact]
    public async Task AddSingle_WhenNewLine_ShouldCreateLineAndReturnSuccess()
    {
        // Arrange
        var userId = Guid.NewGuid();
        var shopId = 1;
        var itemId = Guid.NewGuid();
        var cancellationToken = new CancellationToken();

        var handler = new AddToCartFromFavouritesCommandHandler(_cartService, _favouriteListItems);

        _favouriteListItems
            .ResolveTargetIdsAsync(Arg.Any<AddToCartFromFavouritesCommand>(), Arg.Any<object?>(), cancellationToken)
            .Returns(new List<Guid> { itemId });

        _cartService.AddItemForUserAsync(userId, itemId, cancellationToken)
                    .Returns(CartWith(userId, itemId, qty: 1));

        var command = new AddToCartFromFavouritesCommand(userId, shopId, itemId, isAll: false);

        // Act
        var result = await handler.Handle(command, cancellationToken);

        // Assert
        Assert.True(result.Success);
        Assert.Equal(1, result.TotalQuantity);
        Assert.NotNull(result.AddedItem);

        await _favouriteListItems.Received(1)
            .ResolveTargetIdsAsync(command, Arg.Any<object?>(), cancellationToken);

        await _cartService.Received(1)
            .AddItemForUserAsync(userId, itemId, cancellationToken);

        await _favouriteListItems.DidNotReceiveWithAnyArgs()
            .AddManyToCartAsync(default, default!, default);
    }

    [Fact]
    public async Task AddSingle_WhenServiceThrows_ShouldThrowBadRequest()
    {
        // Arrange
        var userId = Guid.NewGuid();
        var shopId = 1;
        var itemId = Guid.NewGuid();
        var cancellationToken = new CancellationToken();

        var handler = new AddToCartFromFavouritesCommandHandler(_cartService, _favouriteListItems);

        _favouriteListItems
            .ResolveTargetIdsAsync(Arg.Any<AddToCartFromFavouritesCommand>(), Arg.Any<object?>(), cancellationToken)
            .Returns(new List<Guid> { itemId });

        _cartService.AddItemForUserAsync(userId, itemId, cancellationToken)
                    .Throws(new BadRequestException("Item could not be added."));

        var command = new AddToCartFromFavouritesCommand(userId, shopId, itemId, isAll: false);

        // Act & Assert
        await Assert.ThrowsAsync<BadRequestException>(() => handler.Handle(command, cancellationToken));

        await _favouriteListItems.Received(1)
            .ResolveTargetIdsAsync(command, Arg.Any<object?>(), cancellationToken);

        await _favouriteListItems.DidNotReceiveWithAnyArgs()
            .AddManyToCartAsync(default, default!, default);
    }

    [Fact]
    public async Task AddAll_ShouldUseFavouriteListItemsService_AndReturnResult()
    {
        // Arrange
        var userId = Guid.NewGuid();
        var shopId = 1;
        var item1 = Guid.NewGuid();
        var item2 = Guid.NewGuid();
        var cancellationToken = new CancellationToken();

        var handler = new AddToCartFromFavouritesCommandHandler(_cartService, _favouriteListItems);

        _favouriteListItems
            .ResolveTargetIdsAsync(Arg.Any<AddToCartFromFavouritesCommand>(), Arg.Any<object?>(), cancellationToken)
            .Returns(new List<Guid> { item1, item2 });

        var mockCart = CartWith(userId, item1, 2);

        _favouriteListItems.AddManyToCartAsync(userId, Arg.Is<IEnumerable<Guid>>(e => e.Contains(item1) && e.Contains(item2)), cancellationToken)
                            .Returns(mockCart);

        var command = new AddToCartFromFavouritesCommand(userId, shopId, null, isAll: true);

        // Act
        var result = await handler.Handle(command, cancellationToken);

        // Assert
        Assert.True(result.Success);
        Assert.Equal(2, result.TotalQuantity);

        await _favouriteListItems.Received(1)
            .ResolveTargetIdsAsync(command, Arg.Any<object?>(), cancellationToken);

        await _favouriteListItems.Received(1)
            .AddManyToCartAsync(userId, Arg.Is<IEnumerable<Guid>>(e => e.Contains(item1) && e.Contains(item2)), cancellationToken);

        await _cartService.DidNotReceiveWithAnyArgs()
            .AddItemForUserAsync(default, default, default);
    }

    [Fact]
    public async Task AddAll_WhenServiceThrows_ShouldBubbleBadRequest()
    {
        // Arrange
        var userId = Guid.NewGuid();
        var shopId = 1;
        var item1 = Guid.NewGuid();
        var cancellationToken = new CancellationToken();

        var handler = new AddToCartFromFavouritesCommandHandler(_cartService, _favouriteListItems);

        _favouriteListItems
            .ResolveTargetIdsAsync(Arg.Any<AddToCartFromFavouritesCommand>(), Arg.Any<object?>(), cancellationToken)
            .Returns(new List<Guid> { item1 });

        _favouriteListItems
            .AddManyToCartAsync(userId, Arg.Any<IEnumerable<Guid>>(), cancellationToken)
            .Throws(new BadRequestException("No items were added (all unavailable or at maximum quantity)."));

        var command = new AddToCartFromFavouritesCommand(userId, shopId, null, isAll: true);

        // Act & Assert
        await Assert.ThrowsAsync<BadRequestException>(() => handler.Handle(command, cancellationToken));

        await _cartService.DidNotReceiveWithAnyArgs()
            .AddItemForUserAsync(default, default, default);
    }

    [Fact]
    public async Task AddSingle_WhenMissingInventoryId_AllFalse_ShouldThrowBadRequestImmediately()
    {
        // Arrange
        var userId = Guid.NewGuid();
        var cancellationToken = new CancellationToken();

        var handler = new AddToCartFromFavouritesCommandHandler(_cartService, _favouriteListItems);
        var command = new AddToCartFromFavouritesCommand(userId, 1, null, isAll: false);

        // service throws because inventoryItemId is required when all=false
        _favouriteListItems
            .ResolveTargetIdsAsync(command, Arg.Any<object?>(), cancellationToken)
            .Throws(new BadRequestException("inventoryItemId is required when all=false."));

        // Act & Assert
        await Assert.ThrowsAsync<BadRequestException>(() => handler.Handle(command, cancellationToken));

        await _cartService.DidNotReceiveWithAnyArgs()
            .AddItemForUserAsync(default, default, default);
    }

    [Fact]
    public async Task Should_Pass_CancellationToken_ToServices()
    {
        // Arrange
        var userId = Guid.NewGuid();
        var shopId = 1;
        var itemId = Guid.NewGuid();
        var cancellationToken = new CancellationToken();

        var handler = new AddToCartFromFavouritesCommandHandler(_cartService, _favouriteListItems);

        _favouriteListItems
            .ResolveTargetIdsAsync(Arg.Any<AddToCartFromFavouritesCommand>(), Arg.Any<object?>(), cancellationToken)
            .Returns(new List<Guid> { itemId });

        _cartService.AddItemForUserAsync(userId, itemId, cancellationToken)
                    .Returns(CartWith(userId, itemId, 1));

        var command = new AddToCartFromFavouritesCommand(userId, shopId, itemId, isAll: false);

        // Act
        _ = await handler.Handle(command, cancellationToken);

        // Assert
        await _favouriteListItems.Received(1)
            .ResolveTargetIdsAsync(command, Arg.Any<object?>(), cancellationToken);

        await _cartService.Received(1)
            .AddItemForUserAsync(userId, itemId, cancellationToken);

        await _favouriteListItems.DidNotReceiveWithAnyArgs()
            .AddManyToCartAsync(default, default!, default);
    }
}
