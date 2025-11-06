using NettoRetail.Application.CQRS.FavouriteListItems.Commands;
using NettoRetail.Application.Services.FavouriteListItems;
using NettoRetail.Application.Services.ICartService;
using NettoRetail.Domain.Entities;
using NettoRetail.Shared.Specifications;
using NSubstitute;
using NSubstitute.ExceptionExtensions;
using Xunit;

namespace NettoRetail.Tests.UnitTests.FavouriteListTests;

public class AddToCartFromFavouritesCommandHandlerTests
{
    private readonly ICartService _cartService;
    private readonly IFavouriteListItemsService _favouriteListItems;

    public AddToCartFromFavouritesCommandHandlerTests()
    {
        _cartService = Substitute.For<ICartService>();
        _favouriteListItems = Substitute.For<IFavouriteListItemsService>();
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
            .ResolveTargetIdsAsync(
                Arg.Any<AddToCartFromFavouritesCommand>(),
                Arg.Any<Specification<FavouriteListItemEntity>>(),
                cancellationToken)
            .Returns(ci => new List<Guid> { itemId });

        _cartService.AddItemForUserAsync(userId, itemId, cancellationToken)
                    .Returns(CartWith(userId, itemId, 1));

        var command = new AddToCartFromFavouritesCommand(userId, shopId, itemId, isAll: false);

        // Act
        var result = await handler.Handle(command, cancellationToken);

        // Assert
        Assert.True(result.Success);
        Assert.Equal(1, result.TotalQuantity);
        Assert.NotNull(result.AddedItem);
        await _cartService.Received(1).AddItemForUserAsync(userId, itemId, cancellationToken);
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
            .ResolveTargetIdsAsync(
                Arg.Any<AddToCartFromFavouritesCommand>(),
                Arg.Any<Specification<FavouriteListItemEntity>>(),
                cancellationToken)
            .Returns(new List<Guid> { itemId });

        _cartService.AddItemForUserAsync(userId, itemId, cancellationToken)
                    .Throws(new Application.Exceptions.BadRequestException("Item could not be added."));

        var command = new AddToCartFromFavouritesCommand(userId, shopId, itemId, isAll: false);

        // Act & Assert
        await Assert.ThrowsAsync<Application.Exceptions.BadRequestException>(
            () => handler.Handle(command, cancellationToken));
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
            .ResolveTargetIdsAsync(
                Arg.Any<AddToCartFromFavouritesCommand>(),
                Arg.Any<Specification<FavouriteListItemEntity>>(),
                cancellationToken)
            .Returns(new List<Guid> { item1, item2 });

        var cartAfterBatch = CartWith(userId, item1, 2);
        cartAfterBatch.Items.Add(new CartItemEntity { InventoryItemId = item2, Quantity = 1, Price = 50 });

        _favouriteListItems
            .AddManyToCartAsync(userId, Arg.Any<IEnumerable<Guid>>(), cancellationToken)
            .Returns(cartAfterBatch);                                

        var command = new AddToCartFromFavouritesCommand(userId, shopId, null, isAll: true);

        // Act
        var result = await handler.Handle(command, cancellationToken);

        // Assert
        Assert.True(result.Success);
        Assert.Equal(cartAfterBatch.Items.Sum(i => i.Quantity), result.TotalQuantity);
        await _favouriteListItems.Received(1)
            .AddManyToCartAsync(userId, Arg.Any<IEnumerable<Guid>>(), cancellationToken);
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
            .ResolveTargetIdsAsync(
                Arg.Any<AddToCartFromFavouritesCommand>(),
                Arg.Any<Specification<FavouriteListItemEntity>>(),
                cancellationToken)
            .Returns(new List<Guid> { itemId });

        _cartService.AddItemForUserAsync(userId, itemId, cancellationToken)
                    .Returns(CartWith(userId, itemId, 1));

        var command = new AddToCartFromFavouritesCommand(userId, shopId, itemId, isAll: false);

        // Act
        _ = await handler.Handle(command, cancellationToken);

        // Assert
        await _cartService.Received(1).AddItemForUserAsync(userId, itemId, cancellationToken);
    }
}
