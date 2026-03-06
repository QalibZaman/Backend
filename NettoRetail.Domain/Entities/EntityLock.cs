namespace NettoRetail.Domain.Entities;

public class EntityLock
{
    public required string ResourceType { get; set; } // For example "RoleEntity", "AccountEntity"
    public required string ResourceId { get; set; } // entity Id in string (guid, int)
    public required int ShopId { get; set; }
    public required Guid LockedByUserId { get; set; }
    public required DateTimeOffset LockedAtUtc { get; set; }
    public required StaffEntity LockedByUser { get; set; }
}
