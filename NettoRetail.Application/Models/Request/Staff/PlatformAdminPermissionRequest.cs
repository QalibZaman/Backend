using System.ComponentModel;
using System.Text.Json.Serialization;

namespace NettoRetail.Application.Models.Request.Staff;

public class PlatformAdminPermissionRequest
{
    [JsonPropertyName("INVENTORY_MANAGEMENT")]
    [DefaultValue(false)]
    public bool INVENTORY_MANAGEMENT { get; init; }
    [JsonPropertyName("CUSTOMER_MANAGEMENT")]
    [DefaultValue(false)]
    public bool CUSTOMER_MANAGEMENT { get; init; }
    [JsonPropertyName("ORDERS_MANAGEMENT")]
    [DefaultValue(false)]
    public bool ORDERS_MANAGEMENT { get; init; }
    [JsonPropertyName("ADMINS_MANAGEMENT")]
    [DefaultValue(false)]
    public bool ADMINS_MANAGEMENT { get; init; }
    [JsonPropertyName("SYSTEM_MANAGEMENT")]
    [DefaultValue(false)]
    public bool SYSTEM_MANAGEMENT { get; init; }
    [JsonPropertyName("SUPERADMINS_MANAGEMENT")]
    [DefaultValue(false)]
    public bool SUPERADMINS_MANAGEMENT { get; init; }
}