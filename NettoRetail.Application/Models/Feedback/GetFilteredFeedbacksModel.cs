using NettoRetail.Domain.Enums;

namespace NettoRetail.Application.Models.Feedback;

public class GetFilteredFeedbacksModel
{
  public string? ShopName { get; set; }
  public IEnumerable<int>? Ratings { get; set; }
  public DateTime? StartDate { get; set; }
  public DateTime? EndDate { get; set; }
  public IEnumerable<bool>? IsPublic { get; set; }
  public IEnumerable<FeedbackStatusEnum>? Statuses { get; set; }
}
