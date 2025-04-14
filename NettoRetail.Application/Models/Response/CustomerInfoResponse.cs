using NettoRetail.Domain.Enums;

namespace NettoRetail.Application.Models.Response;

public class CustomerInfoResponse
{
    public required Guid CustomerId { get; set; }
    public required string FirstName { get; set; }
    public required string LastName { get; set; }
    public required string Email { get; set; }
    public string? PhoneNumber { get; set; }
    public string? LoyaltyId { get; set; }
    public string? Remarks { get; set; }
}
