namespace NettoRetail.Domain.Entities
{
    public class PaymentEntity
    {
        public int Id { get; set; }
        public required Guid UserId { get; set; }
        public AccountEntity User { get; set; }
        public required string Token { get; set; }
        public DateTime CreatedAt { get; set; }
    }
}
