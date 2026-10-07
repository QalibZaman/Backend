using MediatR;
using NettoRetail.Application.Models.Link;

namespace NettoRetail.Application.CQRS.Shops.Queries
{
    public record GetSocialLinksQuery(int ShopId)
    : IRequest<List<ShopLinkRequest>>;
}
