namespace NettoRetail.Application.Models.Response;
public class PaymentHistoryResponse
{
    public Guid Id { get; set; }
    public DateTime PaymentDate { get; set; }
    public string? CustomerName { get; set; }
    public decimal AmountCharged { get; set; }
    public string? PaymentMethod { get; set; }
    public string? PlanName { get; set; }
    public string? BillingPeriod { get; set; }
    public string? Status { get; set; }
    public string? ShopName { get; set; }
    public string? TransactionId { get; set; }
    public Guid AccountId { get; set; }

}
