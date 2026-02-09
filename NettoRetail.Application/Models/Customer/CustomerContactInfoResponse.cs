namespace NettoRetail.Application.Models.Customer;

public sealed class CustomerContactInfoResponse
{
    public required Guid AccountId { get; set; }
    public required string CustomerName { get; set; }
    public string? Email { get; set; }
    public string? Phone { get; set; }
    public string? LoyaltyId { get; set; }
    public string? Remarks { get; set; }
}