namespace NettoRetail.Application.Models.Request.Review;

public class BulkRejectReviewsRequest
{
    public List<Guid> ReviewIds { get; set; }
    public string Reason { get; set; }
}
