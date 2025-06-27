namespace NettoRetail.Application.Models.Feedback;

public class CreateFeedbackModel
{
  public required int Rating { get; set; }
  public string? Title { get; set; }
  public string? Text { get; set; }
  public bool IsPublic { get; set; } = false;
}