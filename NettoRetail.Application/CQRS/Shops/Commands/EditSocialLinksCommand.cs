using MediatR;
using NettoRetail.Application.Models.Link;

namespace NettoRetail.Application.CQRS.Shops.Commands
{
    public record EditSocialLinksCommand(
        EditSocialLinksRequest Request,
        int ShopId
    ) : IRequest;
}
