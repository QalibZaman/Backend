using NettoRetail.Domain.Entities;

namespace NettoRetail.Application.Models.Response;

public class FeedbackResponse
{
  public Guid Id { get; set; }
  public int Rating { get; set; }
  public string? Title { get; set; }
  public string? Text { get; set; }
  public bool IsPublic { get; set; }
  public string? StaffName { get; set; }
  public RoleEntity? Role { get; set; }
  public DateTime DateSubmitted { get; set; }
  public string? Status { get; set; }
  public string? ShopName { get; set; }
}
