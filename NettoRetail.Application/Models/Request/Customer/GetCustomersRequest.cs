namespace NettoRetail.Application.Models.Request.Customer;

public class GetCustomersRequest
{
    public string? Name { get; set; }
    public string? Email { get; set; }
    public string? LoyaltyId { get; set; }
}
