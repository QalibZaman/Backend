using NettoRetail.Domain.Enums;

namespace NettoRetail.Application.Models.Request.Variation
{
    public class CustomDetailModel
    {
        public required string Name { get; set; }
        public ProductDetailTypeEnum TypeEnum { get; set; }
        public ProductDetailUnitEnum? Unit { get; set; }
        public required object Value { get; set; }
        public required int OrderIndex { get; set; }
    }
}
