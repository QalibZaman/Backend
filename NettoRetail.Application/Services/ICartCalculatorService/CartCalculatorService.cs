using NettoRetail.Application.Models.Response;
using NettoRetail.Domain.Entities;

namespace NettoRetail.Application.Services.ICartCalculatorService;

public class CartCalculatorService : ICartCalculatorService
{
    public decimal CalculateSubtotal(CartEntity cart)
    {
        return cart.TotalPrice;
    }

    public decimal CalculateTotal(CartEntity cart)
    {
        var subtotalPrice = cart.TotalPrice;
        var tax = 0m;
        var shipping = 0m;

        var totalPrice = subtotalPrice + tax + shipping;

        return totalPrice;
    }

    public decimal CalculateTax(CartEntity cart)
    {
        var subtotal = CalculateSubtotal(cart);

        const decimal taxRate = 0m;

        return subtotal * taxRate;
    }

    public decimal CalculateShipping(CartEntity cart) => 0m;

    public CartSummaryResponse GetSummary(CartEntity? cart)
    {
        if (cart == null)
        {
            return new CartSummaryResponse
            {
                Subtotal = 0m,
                Tax = 0m,
                Shipping = 0m,
                Total = 0m,
                TotalQuantity = 0
            };
        }

        var subtotal = CalculateSubtotal(cart);
        var tax = CalculateTax(cart);
        var shipping = CalculateShipping(cart);
        var total = CalculateTotal(cart);

        return new CartSummaryResponse
        {
            Subtotal = subtotal,
            Tax = tax,
            Shipping = shipping,
            Total = total,
            TotalQuantity = cart.TotalQuantity
        };
    }
}
