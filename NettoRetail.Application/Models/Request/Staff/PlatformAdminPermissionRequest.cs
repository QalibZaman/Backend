using System.Text.Json.Serialization;

namespace NettoRetail.Application.Models.Request.Staff;

public class PlatformAdminPermissionRequest
{
    [JsonPropertyName("INVENTORY_MANAGEMENT")]
    public bool INVENTORY_MANAGEMENT { get; init; } = false;
    [JsonPropertyName("CUSTOMER_MANAGEMENT")]
    public bool CUSTOMER_MANAGEMENT { get; init; } = false;
    [JsonPropertyName("ORDERS_MANAGEMENT")]
    public bool ORDERS_MANAGEMENT { get; init; } = false;
    [JsonPropertyName("ADMINS_MANAGEMENT")]
    public bool ADMINS_MANAGEMENT { get; init; } = false;
    [JsonPropertyName("SYSTEM_MANAGEMENT")]
    public bool SYSTEM_MANAGEMENT { get; init; } = false;
    [JsonPropertyName("SUPERADMINS_MANAGEMENT")]
    public bool SUPERADMINS_MANAGEMENT { get; init; } = false;
}