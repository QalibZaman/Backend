namespace NettoRetail.Domain.Entities
{
    public class PaymentMethodEntity
    {
        public int Id { get; set; }
        public required Guid AccountId { get; set; }
        public AccountEntity Account { get; set; }
        public required string StripePaymentMethodId { get; set; }
        public string FourLastCardNumbers { get; set; }
        public string CardBrand { get; set; }
        public long CardExpMonth { get; set; }
        public long CardExpYear { get; set; }
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    }
}
