using NettoRetail.Application.Models.Request.Staff;
using System.Reflection;

namespace NettoRetail.Application.Extensions;

public static class PermissionExtensions
{
    private static readonly PropertyInfo[] BoolProps =
        typeof(PlatformAdminPermissionRequest)
        .GetProperties()
        .Where(p => p.PropertyType == typeof(bool))
        .ToArray();

    public static IEnumerable<string> ToTitles(this PlatformAdminPermissionRequest flags)
    {
        return BoolProps
            .Where(p => (bool)p.GetValue(flags)!)
            .Select(p => p.Name);
    }
}
