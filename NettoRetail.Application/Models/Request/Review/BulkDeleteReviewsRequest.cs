namespace NettoRetail.Application.Models.Request.Review;

public class BulkDeleteReviewsRequest
{
    public List<Guid> ReviewIds { get; set; }
}
