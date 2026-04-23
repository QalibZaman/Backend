namespace NettoRetail.Application.Models.Response;
public class PaymentHistoryResponse
{
    public DateTime PaymentDate { get; set; }
    public required string CustomerName { get; set; }
    public required decimal AmountCharged { get; set; }
    public required string PaymentMethod { get; set; }
    public required string PlanName { get; set; }
    public required string BillingPeriod { get; set; }
    public required string Status { get; set; }
    public required string TransactionId { get; set; }
    public required string? ShopName { get; set; }

}
