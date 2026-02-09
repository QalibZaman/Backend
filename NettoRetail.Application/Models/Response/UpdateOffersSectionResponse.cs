using NettoRetail.Domain.Enums;

namespace NettoRetail.Application.Models.Response
{
    public class UpdateOffersSectionRequest
    {
        public string Name { get; set; }
        public bool IsVisible { get; set; }
        public SelectionTypeEnum SelectionType { get; set; }
        public List<Guid> InventoryItemIds { get; set; } = [];
        public string UpdatedBy { get; set; } = default!;
    }
}
