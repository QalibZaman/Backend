using NettoRetail.Application.Extensions;
using NettoRetail.Domain.Enums;

namespace NettoRetail.Application.Models.Request.Staff;

public class EditPlatformAdminRequest
{
    public List<PlatformAdminPermission> Permissions { get; init; } = [];

    public List<string> GetPermissionTitles()
    {
        return Permissions
            .Select(p => p.GetEnumMemberValue()?.Trim().ToLowerInvariant())
            .Where(v => !string.IsNullOrWhiteSpace(v))
            .Distinct()
            .ToList()!;
    }
}
