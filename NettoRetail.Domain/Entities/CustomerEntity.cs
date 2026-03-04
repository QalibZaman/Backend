namespace NettoRetail.Domain.Entities;

public class CustomerEntity : BaseUserEntity
{
    public bool IsActive { get; set; } = false;
    public bool IsDeleted { get; set; }
    public string? LoyaltyId { get; set; }
    public string? Remarks { get; set; }
    public CartEntity Cart { get; set; } = null!;
    public ICollection<OrderEntity>? Order { get; set; }
}