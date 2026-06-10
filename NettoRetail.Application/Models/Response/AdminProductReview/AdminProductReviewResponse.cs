using NettoRetail.Domain.Enums;

namespace NettoRetail.Application.Models.Response.AdminProductReview;

public class AdminProductReviewResponse
{
    public DateTime DateSubmitted { get; set; }
    public string ProductName { get; set; }
    public string CustomerName { get; set; }
    public int Rating { get; set; }
    public string ReviewText { get; set; }
    public string ReviewStatus { get; set; }
}