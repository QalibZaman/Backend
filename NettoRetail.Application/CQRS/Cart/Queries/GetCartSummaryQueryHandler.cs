using MediatR;
using NettoRetail.Application.Exceptions;
using NettoRetail.Application.Models.Response;
using NettoRetail.Application.Services.ICartCalculatorService;
using NettoRetail.Shared.Interfaces.Repositories;

namespace NettoRetail.Application.CQRS.Cart.Queries;

public record GetCartSummaryQuery(
        Guid CustomerId) : IRequest<CartSummaryResponse>;
public class GetCartSummaryQueryHandler(ICartCalculatorService calculator, ICartRepository cartRepository) : IRequestHandler<GetCartSummaryQuery, CartSummaryResponse>
{
    private readonly ICartCalculatorService _calculator = calculator;
    private readonly ICartRepository _cartRepository = cartRepository;

    public async Task<CartSummaryResponse> Handle(GetCartSummaryQuery request, CancellationToken cancellationToken)
    {
        var cart = await _cartRepository.GetByCustomerIdAsync(request.CustomerId, cancellationToken);

        return _calculator.GetSummary(cart);
    }
}
