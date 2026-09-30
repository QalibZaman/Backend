using NettoRetail.Domain.Entities.Enums;

namespace NettoRetail.Domain.Entities
{
    public class ShopLinkEntity
    {
        public int Id { get; set; }

        public int ShopId { get; set; }

        public required string Url { get; set; }

        public required SocialNetworkType SocialNetwork { get; set; }

        public ShopEntity? Shop { get; set; }
    }
}
