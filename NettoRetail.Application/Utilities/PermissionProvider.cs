using System.Reflection;
using NettoRetail.Shared.Constants;

namespace NettoRetail.Application.Utilities;

public static class PermissionProvider
{
    public static readonly IReadOnlyList<string> All =
        typeof(Permissions)
            .GetFields(BindingFlags.Public | BindingFlags.Static)
            .Where(f => f.IsLiteral && f.FieldType == typeof(string))
            .Select(f => (string)f.GetRawConstantValue()!)
            .ToList();
}
