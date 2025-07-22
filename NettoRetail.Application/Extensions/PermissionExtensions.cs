using NettoRetail.Application.Models.Request.Staff;
using NettoRetail.Shared.Constants;
using System.Reflection;

namespace NettoRetail.Application.Extensions;

public static class PermissionExtensions
{
    private static readonly PropertyInfo[] BoolProps =
        typeof(PlatformAdminPermissionRequest)
        .GetProperties()
        .Where(p => p.PropertyType == typeof(bool))
        .ToArray();

    private static readonly Dictionary<string, string> PermissionMap = typeof(Permissions)
        .GetFields(BindingFlags.Public | BindingFlags.Static)
        .Where(f => f.FieldType == typeof(string))
        .ToDictionary(f => f.Name, f => (string)f.GetValue(null)!);
    public static IEnumerable<string> ToTitles(this PlatformAdminPermissionRequest flags)
    {
        return BoolProps
            .Where(p => (bool)p.GetValue(flags)!)
            .Select(p => PermissionMap[p.Name]);
    }
}
