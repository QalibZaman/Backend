using NettoRetail.Domain.Enums;

namespace NettoRetail.Domain.Entities
{
    public class SubscriptionPlanEntity
    {
        public int Id { get; set; }
        public SubscriptionTypeEnum Type { get; set; }
        public required string Description { get; set; }
        public required decimal MonthlyPrice { get; set; }
        public required string Moto { get; set; }
        public required string SubscriptionId { get; set; }

        public ICollection<AccountEntity>? Accounts { get; set; }
    }
}
