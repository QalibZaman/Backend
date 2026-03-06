using System.Text.Json.Serialization;

namespace NettoRetail.Application.Models.Feedback;

public class BulkRejectFeedbacksRequest
{
    [JsonPropertyName("feedBackIds")]
    public required List<Guid> FeedbackIds { get; set; }
    public required string Reason { get; set; }
}
