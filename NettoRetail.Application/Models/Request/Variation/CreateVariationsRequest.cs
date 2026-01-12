namespace NettoRetail.Application.Models.Request.Variation
{
    public class CreateVariationsRequest
    {
        public Guid InventoryItemId { get; set; }
        public List<VariationModel> Variations { get; set; }
    }
}
