namespace NettoRetail.Application.Models.Feedback;

public class BulkDeleteFeedbacksRequest
{
  public List<Guid> feedbacksIds { get; set; }
}
