using NettoRetail.Infrastructure.Models;

namespace NettoRetail.Application.Models.Response.Comparison
{
    public class ComparisonItemResponse
    {
        public required string ProductName { get; set; }
        public required decimal Price { get; set; }
        public decimal? PromotionPrice { get; set; }
        public string? Brand { get; set; }
        public bool IsPromoted { get; set; }
        public required int AvailableAmount { get; set; }
        public required int ReservedAmount { get; set; }
        public required int OnHandAmount { get; set; }
        public required string ProductLocation { get; set; }
    }
}
