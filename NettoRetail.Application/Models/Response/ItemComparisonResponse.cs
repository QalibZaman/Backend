using NettoRetail.Application.Models.Response.OnboardingSlides;

namespace NettoRetail.Application.Models.Response;

public class ItemComparisonResponse
{
    public Guid Id { get; set; }
    public required List<ItemComparisonItemResponse> ComparisonItems { get; set; }
}
