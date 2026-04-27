using NettoRetail.Domain.Enums;

namespace NettoRetail.Application.Models.InventoryItem;

public class AvailabilityInfoModel
{
    public int AvailableAmount { get; set; }
    public int ReservedAmount { get; set; }
    public int OnHandAmount { get; set; }
    public int IncomingAmount { get; set; }

    public required string ProductLocation { get; set; } = "No location";
    public required InventoryItemStatusEnum Status { get; set; } = InventoryItemStatusEnum.Available;
}