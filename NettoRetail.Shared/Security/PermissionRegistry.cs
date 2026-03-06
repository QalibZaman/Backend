using NettoRetail.Shared.Constants;

namespace NettoRetail.Shared.Security;

public static class PermissionRegistry
{
    public static readonly IReadOnlyDictionary<string, PermissionMetadata> All =
        new Dictionary<string, PermissionMetadata>
        {
            [Permissions.INVENTORY_MANAGEMENT] = new()
            {
                Id = 1,
                Name = Permissions.INVENTORY_MANAGEMENT,
                IsSystemPermission = false
            },

            [Permissions.CUSTOMER_MANAGEMENT] = new()
            {
                Id = 2,
                Name = Permissions.CUSTOMER_MANAGEMENT,
                IsSystemPermission = false
            },

            [Permissions.ORDERS_MANAGEMENT] = new()
            {
                Id = 3,
                Name = Permissions.ORDERS_MANAGEMENT,
                IsSystemPermission = false
            },

            [Permissions.ADMINS_MANAGEMENT] = new()
            {
                Id = 4,
                Name = Permissions.ADMINS_MANAGEMENT,
                IsSystemPermission = false
            },

            [Permissions.SYSTEM_MANAGEMENT] = new()
            {
                Id = 5,
                Name = Permissions.SYSTEM_MANAGEMENT,
                IsSystemPermission = false
            },

            [Permissions.SUPERADMINS_MANAGEMENT] = new()
            {
                Id = 6,
                Name = Permissions.SUPERADMINS_MANAGEMENT,
                IsSystemPermission = true
            }
        };
}
