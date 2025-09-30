using NettoRetail.Application.Services.ICartCalculatorService;
using NettoRetail.Domain.Entities;

namespace NettoRetail.Tests.UnitTests.CartTests;

public class CartCalculatorServiceTests
{
    private readonly CartCalculatorService _calculator;

    public CartCalculatorServiceTests()
    {
        _calculator = new CartCalculatorService();
    }

    [Fact]
    public void GetSummary_ShouldReturnZeros_WhenCartEmpty()
    {
        var cart = new CartEntity { Items = new List<CartItemEntity>() };

        var result = _calculator.GetSummary(cart);

        Assert.Equal(0m, result.Subtotal);
        Assert.Equal(0m, result.Total);
        Assert.Equal(0, result.TotalQuantity);
    }

    [Fact]
    public void GetSummary_ShouldCalculateSubtotalTotalQuantityAndTotal()
    {
        var cart = new CartEntity
        {
            Items = new List<CartItemEntity>
                {
                    new CartItemEntity { Price = 10m, Quantity = 2 },
                    new CartItemEntity { Price = 5m, Quantity = 3 }
                }
        };

        var result = _calculator.GetSummary(cart);

        Assert.Equal(35m, result.Subtotal);   
        Assert.Equal(35m, result.Total);
        Assert.Equal(5, result.TotalQuantity);
    }

    [Fact]
    public void GetSummary_ShouldUsePromotionPrice_IfAvailable()
    {
        var cart = new CartEntity
        {
            Items = new List<CartItemEntity>
                {
                    new CartItemEntity { Price = 20m, PromotionPrice = 15m, Quantity = 2 }
                }
        };

        var result = _calculator.GetSummary(cart);

        Assert.Equal(30m, result.Subtotal);   
        Assert.Equal(30m, result.Total);
        Assert.Equal(2, result.TotalQuantity);
    }

    [Fact]
    public void GetSummary_ShouldMixPromotionAndRegularPrices()
    {
        var cart = new CartEntity
        {
            Items = new List<CartItemEntity>
        {
            new CartItemEntity { Price = 20m, PromotionPrice = 15m, Quantity = 2 },
            new CartItemEntity { Price = 10m, PromotionPrice = null, Quantity = 1 }
        }
        };

        var result = _calculator.GetSummary(cart);

        Assert.Equal(40m, result.Subtotal);  
        Assert.Equal(40m, result.Total);
        Assert.Equal(3, result.TotalQuantity);
    }

    [Fact]
    public void GetSummary_ShouldCalculateWithDecimalPrecision()
    {
        var cart = new CartEntity
        {
            Items = new List<CartItemEntity>
        {
            new CartItemEntity { Price = 9.99m, Quantity = 3 }
        }
        };

        var result = _calculator.GetSummary(cart);

        Assert.Equal(29.97m, result.Subtotal);
        Assert.Equal(29.97m, result.Total);
    }

    [Fact]
    public void GetSummary_ShouldHandleLargeQuantities()
    {
        var cart = new CartEntity
        {
            Items = new List<CartItemEntity>
        {
            new CartItemEntity { Price = 1m, Quantity = 1_000_000 }
        }
        };

        var result = _calculator.GetSummary(cart);

        Assert.Equal(1_000_000m, result.Subtotal);
        Assert.Equal(1_000_000m, result.Total);
        Assert.Equal(1_000_000, result.TotalQuantity);
    }

    [Fact]
    public void GetSummary_ShouldHandleNegativeQuantity()
    {
        var cart = new CartEntity
        {
            Items = new List<CartItemEntity>
        {
            new CartItemEntity { Price = 10m, Quantity = -1 }
        }
        };

        var result = _calculator.GetSummary(cart);

       
        Assert.Equal(-10m, result.Subtotal);
        Assert.Equal(-10m, result.Total);
        Assert.Equal(-1, result.TotalQuantity);
    }

    [Fact]
    public void GetSummary_ShouldReturnZeros_WhenAllQuantitiesZero()
    {
        var cart = new CartEntity
        {
            Items = new List<CartItemEntity>
        {
            new CartItemEntity { Price = 10m, Quantity = 0 },
            new CartItemEntity { Price = 20m, Quantity = 0 }
        }
        };

        var result = _calculator.GetSummary(cart);

        Assert.Equal(0m, result.Subtotal);
        Assert.Equal(0m, result.Total);
        Assert.Equal(0, result.TotalQuantity);
    }

    [Fact]
    public void GetSummary_ShouldIgnoreZeroQuantityItems()
    {
        var cart = new CartEntity
        {
            Items = new List<CartItemEntity>
        {
            new CartItemEntity { Price = 10m, Quantity = 0 },
            new CartItemEntity { Price = 5m, Quantity = 2 }
        }
        };

        var result = _calculator.GetSummary(cart);

        Assert.Equal(10m, result.Subtotal);
        Assert.Equal(10m, result.Total);
        Assert.Equal(2, result.TotalQuantity);
    }

    [Fact]
    public void GetSummary_ShouldUseNegativePromotionPrice_IfProvided()
    {
        var cart = new CartEntity
        {
            Items = new List<CartItemEntity>
        {
            new CartItemEntity { Price = 20m, PromotionPrice = -5m, Quantity = 1 }
        }
        };

        var result = _calculator.GetSummary(cart);

        Assert.Equal(-5m, result.Subtotal);
        Assert.Equal(-5m, result.Total);
        Assert.Equal(1, result.TotalQuantity);
    }

    [Fact]
    public void GetSummary_ShouldCombineSameProduct_WithAndWithoutPromotion()
    {
        var cart = new CartEntity
        {
            Items = new List<CartItemEntity>
        {
            new CartItemEntity { Price = 20m, Quantity = 1 },
            new CartItemEntity { Price = 20m, PromotionPrice = 15m, Quantity = 2 }
        }
        };

        var result = _calculator.GetSummary(cart);

        Assert.Equal(50m, result.Subtotal); 
        Assert.Equal(3, result.TotalQuantity);
    }
}

