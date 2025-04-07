using NettoRetail.Domain.Enums;

namespace NettoRetail.Domain.Entities;

public class CustomerEntity
{
    public required Guid AccountId { get; set; }
    public AccountEntity Account { get; set; }
    public bool IsActive { get; set; } = true;
    public string? LoyaltyId { get; set; }
    public string? Remarks { get; set; }
    public string? PhoneNumber { get; set; }
}
