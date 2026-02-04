namespace NettoRetail.Application.Models.Request.Variation;

public class VariationModel
{
    public string ProductName { get; set; }
    public long ProductUpc { get; set; }
    public bool IsActive { get; set; }
}
