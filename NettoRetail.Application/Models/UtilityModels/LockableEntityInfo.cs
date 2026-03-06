namespace NettoRetail.Application.Models.UtilityModels;

public record LockableEntityInfo
{
    public Type EntityType { get; init; }
    public TimeSpan Ttl { get; init; }
    public LockableEntityInfo(Type entityType, TimeSpan ttl)
    {
        EntityType = entityType;
        Ttl = ttl;
    }
}
