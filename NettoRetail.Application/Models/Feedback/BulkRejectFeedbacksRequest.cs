
namespace NettoRetail.Application.Models.Request.Review;

public class BulkRejectFeedbacksRequest
{
    public required List<Guid> FeedBackIds { get; set; }
    public required string Reason { get; set; }
}
