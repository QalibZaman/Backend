using NettoRetail.Application.CQRS.FavouriteListItems.Commands;
using NettoRetail.Application.Exceptions;
using NettoRetail.Application.Services.ICartService;
using NettoRetail.Domain.Entities;
using NettoRetail.Shared.Interfaces.Repositories;
using NettoRetail.Shared.Specifications;
using NSubstitute;

namespace NettoRetail.Tests.UnitTests.FavouriteListTests;
public class AddToCartFromFavouritesCommandHandlerTests
{
    private readonly IFavouriteListItemRepository _favourites;
    private readonly ICartService _cartService;

    public AddToCartFromFavouritesCommandHandlerTests()
    {
        _favourites = Substitute.For<IFavouriteListItemRepository>();
        _cartService = Substitute.For<ICartService>();
    }

    private static CartEntity CartWith(Guid customerId, Guid itemId, int qty)
        => new()
        {
            CustomerId = customerId,
            Items = new List<CartItemEntity>
            {
                new CartItemEntity
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

        var handler = new AddToCartFromFavouritesCommandHandler(_favourites, _cartService);

        _cartService.AddItemForUserAsync(userId, itemId, Arg.Any<CancellationToken>())
                    .Returns(CartWith(userId, itemId, qty: 1));

        var command = new AddToCartFromFavouritesCommand(userId, shopId, itemId, All: false);

        // Act
        var result = await handler.Handle(command, CancellationToken.None);

        // Assert
        Assert.True(result.Success);
        Assert.Equal(1, result.TotalQuantity);
        Assert.NotNull(result.AddedItem);
        Assert.Equal(itemId, result.AddedItem!.ProductId);
        await _cartService.Received(1).AddItemForUserAsync(userId, itemId, Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task AddSingle_WhenAlreadyInCart_ShouldIncreaseQuantityByOne()
    {
        // Arrange
        var userId = Guid.NewGuid();
        var shopId = 1;
        var itemId = Guid.NewGuid();

        var handler = new AddToCartFromFavouritesCommandHandler(_favourites, _cartService);

        _cartService.AddItemForUserAsync(userId, itemId, Arg.Any<CancellationToken>())
                    .Returns(CartWith(userId, itemId, qty: 2));

        var command = new AddToCartFromFavouritesCommand(userId, shopId, itemId, All: false);

        // Act
        var result = await handler.Handle(command, CancellationToken.None);

        // Assert
        Assert.True(result.Success);
        Assert.Equal(2, result.TotalQuantity);
        await _cartService.Received(1).AddItemForUserAsync(userId, itemId, Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task AddAll_ShouldAddOnlySuccessful_OnFailuresSkipAndContinue()
    {
        // Arrange
        var userId = Guid.NewGuid();
        var shopId = 1;
        var idInStock = Guid.NewGuid();
        var idOos = Guid.NewGuid();

        var handler = new AddToCartFromFavouritesCommandHandler(_favourites, _cartService);

        _favourites.GetManyBySpecAsync(Arg.Any<Specification<FavouriteListItemEntity>>(), Arg.Any<CancellationToken>())
                   .Returns(new List<FavouriteListItemEntity>
                   {
                       new() { AccountId = userId, InventoryItemId = idInStock },
                       new() { AccountId = userId, InventoryItemId = idOos }
                   });

        _cartService.AddItemForUserAsync(userId, idInStock, Arg.Any<CancellationToken>())
                    .Returns(CartWith(userId, idInStock, qty: 1));

        _cartService.AddItemForUserAsync(userId, idOos, Arg.Any<CancellationToken>())
                    .Returns<Task<CartEntity>>(x => throw new BadRequestException("OOS"));

        var command = new AddToCartFromFavouritesCommand(userId, shopId, null, All: true);

        // Act
        var result = await handler.Handle(command, CancellationToken.None);

        // Assert
        Assert.True(result.Success);
        Assert.Equal(1, result.TotalQuantity);
        await _favourites.Received(1).GetManyBySpecAsync(Arg.Any<Specification<FavouriteListItemEntity>>(), Arg.Any<CancellationToken>());
        await _cartService.Received(1).AddItemForUserAsync(userId, idInStock, Arg.Any<CancellationToken>());
        await _cartService.Received(1).AddItemForUserAsync(userId, idOos, Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task AddSingle_WhenServiceThrows_ShouldBubbleBadRequest()
    {
        // Arrange
        var userId = Guid.NewGuid();
        var shopId = 1;
        var itemId = Guid.NewGuid();

        var handler = new AddToCartFromFavouritesCommandHandler(_favourites, _cartService);

        _cartService.AddItemForUserAsync(userId, itemId, Arg.Any<CancellationToken>())
                    .Returns<Task<CartEntity>>(x => throw new BadRequestException("Item could not be added (unavailable or at maximum quantity)."));

        var command = new AddToCartFromFavouritesCommand(userId, shopId, itemId, All: false);

        // Act & Assert
        await Assert.ThrowsAsync<BadRequestException>(() => handler.Handle(command, CancellationToken.None));
    }

    [Fact]
    public async Task AddSingle_WhenMissingInventoryId_AllFalse_ShouldThrowBadRequestImmediately()
    {
        // Arrange
        var userId = Guid.NewGuid();
        var handler = new AddToCartFromFavouritesCommandHandler(_favourites, _cartService);
        var command = new AddToCartFromFavouritesCommand(userId, 1, null, All: false);

        // Act & Assert
        await Assert.ThrowsAsync<BadRequestException>(() => handler.Handle(command, CancellationToken.None));

        await _cartService.DidNotReceive().AddItemForUserAsync(Arg.Any<Guid>(), Arg.Any<Guid>(), Arg.Any<CancellationToken>());
        await _favourites.DidNotReceive().GetManyBySpecAsync(Arg.Any<Specification<FavouriteListItemEntity>>(), Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task AddAll_WhenNoneAdded_ShouldThrowBadRequest()
    {
        // Arrange
        var userId = Guid.NewGuid();
        var shopId = 1;
        var id1 = Guid.NewGuid();
        var id2 = Guid.NewGuid();

        var handler = new AddToCartFromFavouritesCommandHandler(_favourites, _cartService);

        _favourites.GetManyBySpecAsync(Arg.Any<Specification<FavouriteListItemEntity>>(), Arg.Any<CancellationToken>())
                   .Returns(new List<FavouriteListItemEntity>
                   {
                       new() { AccountId = userId, InventoryItemId = id1 },
                       new() { AccountId = userId, InventoryItemId = id2 }
                   });

        _cartService.AddItemForUserAsync(userId, id1, Arg.Any<CancellationToken>())
                    .Returns<Task<CartEntity>>(x => throw new BadRequestException("fail"));
        _cartService.AddItemForUserAsync(userId, id2, Arg.Any<CancellationToken>())
                    .Returns<Task<CartEntity>>(x => throw new BadRequestException("fail"));

        var command = new AddToCartFromFavouritesCommand(userId, shopId, null, All: true);

        // Act & Assert
        await Assert.ThrowsAsync<BadRequestException>(() => handler.Handle(command, CancellationToken.None));
    }

    [Fact]
    public async Task Should_Pass_CancellationToken_ToService_AndRepo()
    {
        // Arrange
        var userId = Guid.NewGuid();
        var shopId = 1;
        var itemId = Guid.NewGuid();
        var cts = new CancellationTokenSource();

        var handler = new AddToCartFromFavouritesCommandHandler(_favourites, _cartService);

        _cartService.AddItemForUserAsync(userId, itemId, cts.Token)
                    .Returns(CartWith(userId, itemId, 1));

        var command = new AddToCartFromFavouritesCommand(userId, shopId, itemId, All: false);

        // Act
        _ = await handler.Handle(command, cts.Token);

        // Assert
        await _cartService.Received(1).AddItemForUserAsync(userId, itemId, cts.Token);
        // No favourites call when All=false
        await _favourites.DidNotReceive().GetManyBySpecAsync(Arg.Any<Specification<FavouriteListItemEntity>>(), Arg.Any<CancellationToken>());
    }
}