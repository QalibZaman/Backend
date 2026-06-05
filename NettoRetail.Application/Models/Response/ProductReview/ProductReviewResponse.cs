namespace NettoRetail.Application.Models.Response.ProductReview;

public class ProductReviewResponse
{
    public Guid Id { get; set; }
    public string ProductUpc { get; set; } = null!;
    public int Rating { get; set; }
    public string Title { get; set; } = null!;
    public string? ReviewText { get; set; }
    public string? VariantInfo { get; set; }
    public string? PhotoUrls { get; set; }
    public DateTime DateSubmitted { get; set; }
    public string ReviewStatus { get; set; } = null!;
}
