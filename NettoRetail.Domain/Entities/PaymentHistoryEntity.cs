namespace NettoRetail.Domain.Entities;
public class PaymentHistoryEntity
{
    public Guid AccountId { get; set; }
    public DateTime PaymentDate { get; set; }
    public string CustomerName { get; set; }
    public decimal AmountCharged { get; set; }
    public string PaymentMethod { get; set; }
    public string PlanName { get; set; }
    public string BillingPeriod { get; set; }
    public string Status { get; set; }
    public string TransactionId { get; set; }

    public AccountEntity Account { get; set; }
}
