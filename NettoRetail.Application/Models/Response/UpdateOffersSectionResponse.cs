using NettoRetail.Domain.Enums;

namespace NettoRetail.Application.Models.Response
{
    public class UpdateOffersSectionResponse
    {
        public string Name { get; set; }
        public string? Preview { get; set; }
        public bool IsVisible { get; set; }
        public SelectionTypeEnum SelectionType { get; set; }
        public List<int> ProductIds { get; set; } = [];
        public string UpdatedBy { get; set; }

    }
}
