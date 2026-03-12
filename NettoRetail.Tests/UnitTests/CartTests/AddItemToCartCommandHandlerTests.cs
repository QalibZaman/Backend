using MapsterMapper;
using NettoRetail.Application.CQRS.Cart.Commands;
using NettoRetail.Application.Exceptions;
using NettoRetail.Application.Services.ICartService;
using NettoRetail.Domain.Entities;
using NettoRetail.Tests.Helpers;
using NSubstitute;
using NSubstitute.ExceptionExtensions;

namespace NettoRetail.Tests.UnitTests.CartTests;

public class AddItemToCartCommandHandlerTests
{
    private readonly IMapper _mapper = MapperFactory.Create();

    [Fact]
    public async Task Handle_ShouldReturnResponse_WithAddedItemDetails()
    {
        var cartService = Substitute.For<ICartService>();
        var customerId = Guid.NewGuid();
        var productId = Guid.NewGuid();

        var command = new AddItemToCartCommand(productId, customerId);

        var cart = new CartEntity
        {
            Id = Guid.NewGuid(),
            CustomerId = customerId,
            Items = new List<CartItemEntity>
                {
                    new CartItemEntity
                    {
                        InventoryItemId = productId,
                        ProductUpc = "123456",
                        ProductName = "Test Product",
                        Price = 10m,
                        Quantity = 1,
                        Size = "M",
                        PromotionPrice = null
                    }
                }
        };

        cartService.AddItemForUserAsync(customerId, productId, Arg.Any<CancellationToken>())
            .Returns(cart);

        var handler = new AddItemToCartCommandHandler(cartService, _mapper);

        var result = await handler.Handle(command, CancellationToken.None);

        Assert.True(result.Success);
        Assert.Equal("Added to bag", result.Message);
        Assert.Equal(1, result.TotalQuantity);
        Assert.Equal("123456", result.AddedItem.ProductUpc);
        Assert.Equal("Test Product", result.AddedItem.ProductName);
        Assert.Equal(10m, result.AddedItem.Price);
        Assert.Equal(1, result.AddedItem.Quantity);
        Assert.Equal("M", result.AddedItem.Size);
        Assert.Null(result.AddedItem.PromotionPrice);

        await cartService.Received(1).AddItemForUserAsync(customerId, productId, Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task Handle_ShouldAggregateTotalQuantity_WhenMultipleItemsExist()
    {
        var cartService = Substitute.For<ICartService>();
        var customerId = Guid.NewGuid();
        var productId = Guid.NewGuid();

        var command = new AddItemToCartCommand(productId, customerId);

        var cart = new CartEntity
        {
            CustomerId = customerId,
            Items = new List<CartItemEntity>
                {
                    new CartItemEntity { InventoryItemId = productId, ProductName = "Test Product", Price = 10m, Quantity = 2 },
                    new CartItemEntity { InventoryItemId = Guid.NewGuid(), ProductName = "Other Product", Price = 20m, Quantity = 3 }
                }
        };

        cartService.AddItemForUserAsync(customerId, productId, Arg.Any<CancellationToken>())
            .Returns(cart);

        var handler = new AddItemToCartCommandHandler(cartService, _mapper);

        var result = await handler.Handle(command, CancellationToken.None);

        Assert.True(result.Success);
        Assert.Equal(5, result.TotalQuantity);
    }

    [Fact]
    public async Task Handle_ShouldThrowNotFoundException_WhenProductDoesNotExist()
    {
        var cartService = Substitute.For<ICartService>();
        var customerId = Guid.NewGuid();
        var productId = Guid.NewGuid();

        var command = new AddItemToCartCommand(productId, customerId);

        cartService.AddItemForUserAsync(customerId, productId, Arg.Any<CancellationToken>())
            .Throws(new NotFoundException("Product not found"));

        var handler = new AddItemToCartCommandHandler(cartService, _mapper);

        await Assert.ThrowsAsync<NotFoundException>(() =>
            handler.Handle(command, CancellationToken.None));
    }

    [Fact]
    public async Task Handle_ShouldThrowBadRequestException_WhenStockExceeded()
    {
        var cartService = Substitute.For<ICartService>();
        var customerId = Guid.NewGuid();
        var productId = Guid.NewGuid();

        var command = new AddItemToCartCommand(productId, customerId);

        cartService.AddItemForUserAsync(customerId, productId, Arg.Any<CancellationToken>())
            .Throws(new BadRequestException("Quantity exceeds available stock"));

        var handler = new AddItemToCartCommandHandler(cartService, _mapper);

        await Assert.ThrowsAsync<BadRequestException>(() =>
            handler.Handle(command, CancellationToken.None));
    }

    [Fact]
    public async Task Handle_ShouldMapOnlyRequestedItem_WhenCartHasMultipleItems()
    {
        var cartService = Substitute.For<ICartService>();
        var customerId = Guid.NewGuid();
        var requestedProductId = Guid.NewGuid();
        var otherProductId = Guid.NewGuid();

        var command = new AddItemToCartCommand(requestedProductId, customerId);

        var cart = new CartEntity
        {
            CustomerId = customerId,
            Items = new List<CartItemEntity>
                {
                    new CartItemEntity
                    {
                        InventoryItemId = otherProductId,
                        ProductName = "Other Product",
                        Price = 20m,
                        Quantity = 3
                    },
                    new CartItemEntity
                    {
                        InventoryItemId = requestedProductId,
                        ProductName = "Requested Product",
                        Price = 10m,
                        Quantity = 1
                    }
                }
        };

        cartService.AddItemForUserAsync(customerId, requestedProductId, Arg.Any<CancellationToken>())
            .Returns(cart);

        var handler = new AddItemToCartCommandHandler(cartService, _mapper);

        var result = await handler.Handle(command, CancellationToken.None);

        Assert.Equal("Requested Product", result.AddedItem.ProductName);
        Assert.Equal(10m, result.AddedItem.Price);
        Assert.Equal(1, result.AddedItem.Quantity);
    }

    [Fact]
    public async Task Handle_ShouldThrow_WhenCartHasNoItems()
    {
        var cartService = Substitute.For<ICartService>();
        var customerId = Guid.NewGuid();
        var productId = Guid.NewGuid();

        var command = new AddItemToCartCommand(productId, customerId);

        var emptyCart = new CartEntity
        {
            CustomerId = customerId,
            Items = new List<CartItemEntity>()
        };

        cartService.AddItemForUserAsync(customerId, productId, Arg.Any<CancellationToken>())
            .Returns(emptyCart);

        var handler = new AddItemToCartCommandHandler(cartService, _mapper);

        await Assert.ThrowsAsync<InvalidOperationException>(() =>
            handler.Handle(command, CancellationToken.None));
    }
}
