namespace NettoRetail.Application.Models.Shop;

public class DeactivateShopRequest
{
    public required string Reason { get; set; }
    public DateTime DeactivationDate { get; set; }
}
