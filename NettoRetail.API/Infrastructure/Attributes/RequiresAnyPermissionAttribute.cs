namespace NettoRetail.API.Infrastructure.Attributes;

[AttributeUsage(AttributeTargets.Class | AttributeTargets.Method, AllowMultiple = true)]
public class RequiresAnyPermissionAttribute : Attribute
{
    public string[] Permissions { get; }

    public RequiresAnyPermissionAttribute(params string[] permissions)
    {
        Permissions = permissions;
    }
}