using NettoRetail.Domain.Enums;

namespace NettoRetail.Domain.Entities;

public class CustomerEntity
{
    public required Guid AccountId { get; set; }
    public AccountEntity Account { get; set; }
    public bool IsActive { get; set; } = true;
    public bool IsDeleted { get; set; }
    public string? LoyaltyId { get; set; }
    public string? Remarks { get; set; }
    public string? PhoneNumber { get; set; }
    public CartEntity Cart { get; set; } = null!;
    public ICollection<OrderEntity>? Order { get; set; }
}