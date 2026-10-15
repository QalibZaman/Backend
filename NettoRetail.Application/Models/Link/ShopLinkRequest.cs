using NettoRetail.Domain.Entities.Enums;

namespace NettoRetail.Application.Models.Link
{
    public class ShopLinkRequest
    {
        public required SocialNetworkType SocialNetwork { get; set; }
        public required string Url { get; set; }
    }
}
