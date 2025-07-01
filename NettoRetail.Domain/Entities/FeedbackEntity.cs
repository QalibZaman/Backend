using NettoRetail.Domain.Enums;

namespace NettoRetail.Domain.Entities;

public class FeedbackEntity
{
  public Guid Id { get; set; } = Guid.NewGuid();
  public required int Rating { get; set; }
  public required string ShopName { get; set; }
  public required string StaffName { get; set; }
  public string? Title { get; set; }
  public string? Text { get; set; }
  public bool IsPublic { get; set; } = false;
  public Guid? AccountId { get; set; }
  public AccountEntity Account { get; set; } = null!;
  public DateTime DateSubmitted { get; set; } = DateTime.UtcNow;
  public FeedbackStatusEnum Status { get; set; } = FeedbackStatusEnum.Pending;
  }
