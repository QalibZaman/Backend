using Mapster;
using MediatR;
using NettoRetail.Application.Models.Response;
using NettoRetail.Shared.Interfaces.Repositories;

namespace NettoRetail.Application.CQRS.Cart.Queries;

public record GetCartItemsQuery(Guid CustomerId) : IRequest<GetCartItemsResponse>;

public class GetCartItemQueryHandler(ICartRepository cartRepository) : IRequestHandler<GetCartItemsQuery, GetCartItemsResponse>
{
    public async Task<GetCartItemsResponse> Handle(GetCartItemsQuery request, CancellationToken cancellationToken)
    {
        var cart = await cartRepository.GetCartWithItemsAsync(request.CustomerId, cancellationToken);

        var result = cart.Adapt<GetCartItemsResponse>();

        return result;
    }
}