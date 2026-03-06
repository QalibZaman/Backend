using System.Text.Json.Serialization;

namespace NettoRetail.Application.Models.Feedback;

public class BulkDeleteFeedbacksRequest
{
  [JsonPropertyName("feedbacksIds")]
  public required List<Guid> FeedbackIds { get; set; }
}
