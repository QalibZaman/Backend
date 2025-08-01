using NettoRetail.Application.Models.Request.Staff;
using NettoRetail.Shared.Constants;
using System.Reflection;
using System.Runtime.Serialization;

namespace NettoRetail.Application.Extensions;

public static class PermissionExtensions
{
  public static string? GetEnumMemberValue<T>(this T enumValue)
      where T : Enum
  {
    return typeof(T)
           .GetMember(enumValue.ToString())
           .FirstOrDefault()?
           .GetCustomAttribute<EnumMemberAttribute>()?
           .Value;
  }
}