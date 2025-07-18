using System.Runtime.Serialization;

namespace NettoRetail.Domain.Enums;

public enum PlatformAdminPermission
{
    [EnumMember(Value = "Inventory Management")]
    INVENTORY_MANAGEMENT,
    [EnumMember(Value = "Customer Management")]
    CUSTOMER_MANAGEMENT,
    [EnumMember(Value = "Orders Management")]
    ORDERS_MANAGEMENT,
    [EnumMember(Value = "Admins Management")]
    ADMINS_MANAGEMENT,
    [EnumMember(Value = "System Management")]
    SYSTEM_MANAGEMENT,
    [EnumMember(Value = "SuperAdmins Management")]
    SUPERADMINS_MANAGEMENT
}
