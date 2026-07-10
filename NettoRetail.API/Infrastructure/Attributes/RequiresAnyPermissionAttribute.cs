namespace NettoRetail.API.Infrastructure.Attributes;

[AttributeUsage(AttributeTargets.Class | AttributeTargets.Method, AllowMultiple = true)]
public class RequiresAnyPermissionAttribute : Attribute
{
    public string[] Permissions { get; }

    public RequiresAnyPermissionAttribute(params string[] permissions)
    {
        if (permissions == null || permissions.Length == 0)
            throw new ArgumentException("At least one permission must be specified");

        Permissions = permissions;
    }
}