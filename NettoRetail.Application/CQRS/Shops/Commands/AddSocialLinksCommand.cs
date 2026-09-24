using MediatR;
using NettoRetail.Application.Models.Link;

namespace NettoRetail.Application.CQRS.Shops.Commands
{
    public record AddSocialLinksCommand(
    AddSocialLinksRequest Request,
    int ShopId
) : IRequest;
}
