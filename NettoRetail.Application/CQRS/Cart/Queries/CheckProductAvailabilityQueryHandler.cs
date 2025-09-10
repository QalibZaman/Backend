using MediatR;
using NettoRetail.Application.Models.Response;
using NettoRetail.Application.Services.ICartService;

namespace NettoRetail.Application.CQRS.Cart.Queries
{
    public record CheckProductAvailabilityQuery(long ProductUpc) : IRequest<ProductAvailabilityResponse>;

    public class CheckProductAvailabilityQueryHandler
     : IRequestHandler<CheckProductAvailabilityQuery, ProductAvailabilityResponse>
    {
        private readonly ICartService _cartService;

        public CheckProductAvailabilityQueryHandler(ICartService cartService)
        {
            _cartService = cartService;
        }

        public async Task<ProductAvailabilityResponse> Handle(CheckProductAvailabilityQuery request, CancellationToken cancellationToken)
        {
            try
            {
                var inStock = await _cartService.IsProductAvailableAsync(request.ProductUpc, cancellationToken);

                return new ProductAvailabilityResponse
                {
                    Success = true,
                    InStock = inStock,
                    Message = inStock ? "Available" : "Out of stock"
                };
            }
            catch
            {
                return new ProductAvailabilityResponse
                {
                    Success = false,
                    InStock = false,
                    Message = "Something went wrong. Please try again later."
                };
            }
        }
    }
}
