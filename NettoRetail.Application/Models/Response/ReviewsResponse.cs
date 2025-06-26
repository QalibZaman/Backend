using NettoRetail.Domain.Enums;

namespace NettoRetail.Application.Models.Response;
public class ReviewResponse
{
    public Guid Id { get; set; }
    public required string ShopName { get; set; }
    public required string StaffName { get; set; }
    public required string Role { get; set; }
    public required int Rating { get; set; }
    public required string ReviewTextPreview { get; set; }
    public required DateTime DateSubmitted { get; set; }
    public required ReviewStatusEnum Status { get; set; }
}
