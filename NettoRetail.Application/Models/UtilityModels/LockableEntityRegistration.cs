namespace NettoRetail.Application.Models.UtilityModels;

public record LockableEntityRegistration
{
    public Type EntityType { get; set; }
    public TimeSpan Ttl { get; init; }
    public Func<string, int, CancellationToken, Task<bool>> ExistsFunc { get; init; }

    public LockableEntityRegistration(
        Type entityType,
        TimeSpan ttl,
        Func<string, int, CancellationToken, Task<bool>> existsFunc)
    {
        EntityType = entityType;
        Ttl = ttl;
        ExistsFunc = existsFunc;
    }
}
