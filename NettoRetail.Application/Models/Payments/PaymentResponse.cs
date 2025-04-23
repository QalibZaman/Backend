namespace NettoRetail.Application.Models.Payments;

public class PaymentResponse
{
    public int Id { get; set; }
    public required string UserId { get; set; }
    public required string Token { get; set; }
    public DateTime CreatedAt { get; set; }
}
