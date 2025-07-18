
namespace NettoRetail.Application.Models.Response;

public class PlatformAdminPermissionResponse
{
    public bool INVENTORY_MANAGEMENT { get; init; }
    public bool CUSTOMER_MANAGEMENT { get; init; }
    public bool ORDERS_MANAGEMENT { get; init; }
    public bool ADMINS_MANAGEMENT { get; init; }
    public bool SYSTEM_MANAGEMENT { get; init; }
    public bool SUPERADMINS_MANAGEMENT { get; init; }
}
