using NettoRetail.Application.CQRS.FavouriteListItems.Commands;
using NettoRetail.Application.Exceptions;
using NettoRetail.Application.Services.ICartService;
using NettoRetail.Application.Services.IFavouriteListItemsService;
using NettoRetail.Domain.Entities;
using NettoRetail.Shared.Interfaces.Repositories;
using NettoRetail.Shared.Specifications;
using NSubstitute;
using NSubstitute.ExceptionExtensions;

namespace NettoRetail.Tests.UnitTests.FavouriteListTests;

public class AddToCartFromFavouritesCommandHandlerTests
{
    private readonly IFavouriteListItemRepository _favourites;
    private readonly ICartService _cartService;
    private readonly IFavouriteListItems _favouriteListItems;

    public AddToCartFromFavouritesCommandHandlerTests()
    {
        _favourites = Substitute.For<IFavouriteListItemRepository>();
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
        var userId = Guid.NewGuid();
        var shopId = 1;
        var itemId = Guid.NewGuid();
        var cancellationToken = new CancellationToken();

        var handler = new AddToCartFromFavouritesCommandHandler(_favourites, _cartService, _favouriteListItems);

        _cartService.AddItemForUserAsync(userId, itemId, cancellationToken)
                    .Returns(CartWith(userId, itemId, qty: 1));

        var command = new AddToCartFromFavouritesCommand(userId, shopId, itemId, isAll: false);

        var result = await handler.Handle(command, cancellationToken);

        Assert.True(result.Success);
        Assert.Equal(1, result.TotalQuantity);
        Assert.NotNull(result.AddedItem);
        await _cartService.Received(1).AddItemForUserAsync(userId, itemId, cancellationToken);
        await _favouriteListItems.DidNotReceiveWithAnyArgs().AddManyToCartAsync(default, default, default, default);
    }

    [Fact]
    public async Task AddSingle_WhenServiceThrows_ShouldThrowBadRequest()
    {
        var userId = Guid.NewGuid();
        var shopId = 1;
        var itemId = Guid.NewGuid();
        var cancellationToken = new CancellationToken();

        var handler = new AddToCartFromFavouritesCommandHandler(_favourites, _cartService, _favouriteListItems);

        _cartService.AddItemForUserAsync(userId, itemId, cancellationToken)
                    .Throws(new BadRequestException("Item could not be added."));

        var command = new AddToCartFromFavouritesCommand(userId, shopId, itemId, isAll: false);

        await Assert.ThrowsAsync<BadRequestException>(() => handler.Handle(command, cancellationToken));

        await _favouriteListItems.DidNotReceiveWithAnyArgs().AddManyToCartAsync(default, default, default, default);
    }

    [Fact]
    public async Task AddAll_ShouldUseFavouriteListItemsService_AndReturnResult()
    {
        var userId = Guid.NewGuid();
        var shopId = 1;
        var item1 = Guid.NewGuid();
        var item2 = Guid.NewGuid();
        var cancellationToken = new CancellationToken();

        var handler = new AddToCartFromFavouritesCommandHandler(_favourites, _cartService, _favouriteListItems);

        _favourites.GetManyBySpecAsync(Arg.Any<Specification<FavouriteListItemEntity>>(), cancellationToken)
                   .Returns(new List<FavouriteListItemEntity>
                   {
                       new() { AccountId = userId, InventoryItemId = item1 },
                       new() { AccountId = userId, InventoryItemId = item2 }
                   });

        var mockCart = CartWith(userId, item1, 2);

        _favouriteListItems.AddManyToCartAsync(userId, Arg.Any<IEnumerable<Guid>>(), Arg.Any<int>(), cancellationToken)
                            .Returns(new FavouriteListBulkAddResult(2, mockCart, mockCart.Items.First()));

        var command = new AddToCartFromFavouritesCommand(userId, shopId, null, isAll: true);

        var result = await handler.Handle(command, cancellationToken);

        Assert.True(result.Success);
        Assert.Equal(2, result.TotalQuantity);
        await _favouriteListItems.Received(1)
            .AddManyToCartAsync(userId, Arg.Any<IEnumerable<Guid>>(), Arg.Any<int>(), cancellationToken);
    }

    [Fact]
    public async Task AddAll_WhenServiceReturnsZero_ShouldThrowBadRequest()
    {
        var userId = Guid.NewGuid();
        var shopId = 1;
        var item1 = Guid.NewGuid();
        var cancellationToken = new CancellationToken();

        var handler = new AddToCartFromFavouritesCommandHandler(_favourites, _cartService, _favouriteListItems);

        _favourites.GetManyBySpecAsync(Arg.Any<Specification<FavouriteListItemEntity>>(), cancellationToken)
                   .Returns(new List<FavouriteListItemEntity>
                   {
                       new() { AccountId = userId, InventoryItemId = item1 }
                   });

        _favouriteListItems.AddManyToCartAsync(userId, Arg.Any<IEnumerable<Guid>>(), Arg.Any<int>(), cancellationToken)
                            .Returns(new FavouriteListBulkAddResult(0, null, null));

        var command = new AddToCartFromFavouritesCommand(userId, shopId, null, isAll: true);

        await Assert.ThrowsAsync<BadRequestException>(() => handler.Handle(command, cancellationToken));
    }

    [Fact]
    public async Task AddSingle_WhenMissingInventoryId_AllFalse_ShouldThrowBadRequestImmediately()
    {
        var userId = Guid.NewGuid();
        var cancellationToken = new CancellationToken();

        var handler = new AddToCartFromFavouritesCommandHandler(_favourites, _cartService, _favouriteListItems);
        var command = new AddToCartFromFavouritesCommand(userId, 1, null, isAll: false);

        await Assert.ThrowsAsync<BadRequestException>(() => handler.Handle(command, cancellationToken));

        await _cartService.DidNotReceive().AddItemForUserAsync(Arg.Any<Guid>(), Arg.Any<Guid>(), cancellationToken);
        await _favouriteListItems.DidNotReceiveWithAnyArgs().AddManyToCartAsync(default, default, default, default);
    }

    [Fact]
    public async Task Should_Pass_CancellationToken_ToService_AndRepo()
    {
        var userId = Guid.NewGuid();
        var shopId = 1;
        var itemId = Guid.NewGuid();
        var cancellationToken = new CancellationToken();

        var handler = new AddToCartFromFavouritesCommandHandler(_favourites, _cartService, _favouriteListItems);

        _cartService.AddItemForUserAsync(userId, itemId, cancellationToken)
                    .Returns(CartWith(userId, itemId, 1));

        var command = new AddToCartFromFavouritesCommand(userId, shopId, itemId, isAll: false);

        _ = await handler.Handle(command, cancellationToken);

        await _cartService.Received(1).AddItemForUserAsync(userId, itemId, cancellationToken);
        await _favouriteListItems.DidNotReceiveWithAnyArgs().AddManyToCartAsync(default, default, default, default);
    }
}
