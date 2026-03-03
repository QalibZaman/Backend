namespace NettoRetail.Shared.Security;

public class PermissionMetadata
{
    public int Id { get; init; }
    public string Name { get; init; } = default!;
    public bool IsSystemPermission { get; init; }
}
