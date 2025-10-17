using NettoRetail.Application.CQRS.Cart.Queries;
using NettoRetail.Application.Exceptions;
using NettoRetail.Application.Models.Response;
using NettoRetail.Application.Services.ICartCalculatorService;
using NettoRetail.Domain.Entities;
using NettoRetail.Shared.Interfaces.Repositories;
using NSubstitute;
using NSubstitute.ExceptionExtensions;

namespace NettoRetail.Tests.UnitTests.CartTests;
public class CartSummaryQueryHandlerTests
{
    private readonly ICartCalculatorService _calculator;
    private readonly ICartRepository _cartRepo;

    public CartSummaryQueryHandlerTests()
    {
        _calculator = Substitute.For<ICartCalculatorService>();
        _cartRepo = Substitute.For<ICartRepository>();
    }

    [Fact]
    public async Task Handle_ShouldReturnSummary_WhenCartExists()
    {
        var customerId = Guid.NewGuid();
        var cart = new CartEntity
        {
            CustomerId = customerId,
            Items = new List<CartItemEntity>
                {
                    new CartItemEntity { Price = 10m, Quantity = 2 },
                    new CartItemEntity { Price = 20m, Quantity = 1 }
                }
        };

        var expectedSummary = new CartSummaryResponse
        {
            Subtotal = 40m,
            Tax = 0m,
            Shipping = 0m,
            Total = 40m,
            TotalQuantity = 3
        };

        _cartRepo.GetByCustomerIdAsync(customerId, Arg.Any<CancellationToken>())
            .Returns(cart);

        _calculator.GetSummary(cart).Returns(expectedSummary);

        var handler = new GetCartSummaryQueryHandler(_calculator, _cartRepo);

        var result = await handler.Handle(new GetCartSummaryQuery(customerId), CancellationToken.None);

        Assert.Equal(40m, result.Subtotal);
        Assert.Equal(40m, result.Total);
        Assert.Equal(3, result.TotalQuantity);
    }


    [Fact]
    public async Task Handle_ShouldCallCalculatorExactlyOnce_WhenCartExists()
    {
        var customerId = Guid.NewGuid();
        var cart = new CartEntity { CustomerId = customerId };

        var expectedSummary = new CartSummaryResponse { Subtotal = 10, Total = 10, TotalQuantity = 1 };

        _cartRepo.GetByCustomerIdAsync(customerId, Arg.Any<CancellationToken>())
            .Returns(cart);

        _calculator.GetSummary(cart).Returns(expectedSummary);

        var handler = new GetCartSummaryQueryHandler(_calculator, _cartRepo);

        var result = await handler.Handle(new GetCartSummaryQuery(customerId), CancellationToken.None);

        Assert.Equal(expectedSummary, result);
        _calculator.Received(1).GetSummary(cart);
    }

    [Fact]
    public async Task Handle_ShouldPassCancellationToken_ToRepository()
    {
        var customerId = Guid.NewGuid();
        var cts = new CancellationTokenSource();

        var handler = new GetCartSummaryQueryHandler(_calculator, _cartRepo);

        await handler.Handle(new GetCartSummaryQuery(customerId), cts.Token);

        await _cartRepo.Received(1).GetByCustomerIdAsync(customerId, cts.Token);
    }

    [Fact]
    public async Task Handle_ShouldReturnCalculatorResponseDirectly()
    {
        var customerId = Guid.NewGuid();
        var cart = new CartEntity { CustomerId = customerId };

        var expected = new CartSummaryResponse
        {
            Subtotal = 123.45m,
            Tax = 12.34m,
            Shipping = 5.67m,
            Total = 141.46m,
            TotalQuantity = 10
        };

        _cartRepo.GetByCustomerIdAsync(customerId, Arg.Any<CancellationToken>())
            .Returns(cart);

        _calculator.GetSummary(cart).Returns(expected);

        var handler = new GetCartSummaryQueryHandler(_calculator, _cartRepo);

        var result = await handler.Handle(new GetCartSummaryQuery(customerId), CancellationToken.None);

        Assert.Same(expected, result); 
    }

    [Fact]
    public async Task Handle_ShouldReturnZeroSummary_WhenCartDoesNotExist()
    {
        var customerId = Guid.NewGuid();

        _cartRepo.GetByCustomerIdAsync(customerId, Arg.Any<CancellationToken>())
            .Returns((CartEntity?)null);

        var expected = new CartSummaryResponse
        {
            Subtotal = 0m,
            Tax = 0m,
            Shipping = 0m,
            Total = 0m,
            TotalQuantity = 0
        };

        _calculator.GetSummary(null).Returns(expected);

        var handler = new GetCartSummaryQueryHandler(_calculator, _cartRepo);

        var result = await handler.Handle(new GetCartSummaryQuery(customerId), CancellationToken.None);

        Assert.Equal(0m, result.Subtotal);
        Assert.Equal(0m, result.Total);
        Assert.Equal(0, result.TotalQuantity);
    }

    [Fact]
    public async Task Handle_ShouldPropagateException_WhenCalculatorFails()
    {
        var customerId = Guid.NewGuid();
        var cart = new CartEntity { CustomerId = customerId };

        _cartRepo.GetByCustomerIdAsync(customerId, Arg.Any<CancellationToken>())
            .Returns(cart);

        _calculator.GetSummary(cart).Throws(new InvalidOperationException("Calculation failed"));

        var handler = new GetCartSummaryQueryHandler(_calculator, _cartRepo);

        await Assert.ThrowsAsync<InvalidOperationException>(() =>
            handler.Handle(new GetCartSummaryQuery(customerId), CancellationToken.None));
    }

    [Fact]
    public async Task Handle_ShouldPropagateException_WhenRepositoryFails()
    {
        var customerId = Guid.NewGuid();

        _cartRepo.GetByCustomerIdAsync(customerId, Arg.Any<CancellationToken>())
            .Throws(new Exception("DB error"));

        var handler = new GetCartSummaryQueryHandler(_calculator, _cartRepo);

        await Assert.ThrowsAsync<Exception>(() =>
            handler.Handle(new GetCartSummaryQuery(customerId), CancellationToken.None));
    }
}
