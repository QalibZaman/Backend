namespace NettoRetail.Application.Models.Request.Variation;

public class VariationModel
{
    public string ProductName { get; set; }
    public string ProductUpc { get; set; } = string.Empty;
    public bool IsActive { get; set; }
}
