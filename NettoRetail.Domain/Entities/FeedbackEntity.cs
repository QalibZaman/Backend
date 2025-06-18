using NettoRetail.Domain.Enums;

namespace NettoRetail.Domain.Entities;

public class FeedbackEntity
{
  public Guid Id { get; set; } = Guid.NewGuid();
  public required int Rating { get; set; }
  public string? Title { get; set; }
  public string? Text { get; set; }
  public bool IsPublic { get; set; } = false;
  public required int ShopId { get; set; }
  public ShopEntity? Shop { get; set; }
  public string StaffName { get; set; } = "Anonymous";
  public string? Role { get; set; }
  public DateTime DateSubmitted { get; set; } = DateTime.UtcNow;
  public FeedbackStatusEnum Status { get; set; } = FeedbackStatusEnum.Pending;
}
