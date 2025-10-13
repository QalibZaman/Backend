using NettoRetail.Application.Models.Response;
using NettoRetail.Domain.Entities;

namespace NettoRetail.Application.Services.ICartCalculatorService;

public interface ICartCalculatorService
{
    decimal CalculateSubtotal(CartEntity cart);
    decimal CalculateTax(CartEntity cart);
    decimal CalculateShipping(CartEntity cart);
    decimal CalculateTotal(CartEntity cart);
    CartSummaryResponse GetSummary(CartEntity cart);
}
