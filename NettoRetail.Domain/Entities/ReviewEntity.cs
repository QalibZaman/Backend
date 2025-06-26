using NettoRetail.Domain.Enums;

namespace NettoRetail.Domain.Entities;

public class ReviewEntity
{
    public Guid Id { get; set; }
    public required string ShopName { get; set; }
    public required string StaffName { get; set; }
    public required string Role { get; set; }
    public required int Rating { get; set; }
    public required string ReviewText { get; set; }
    public string? RejectionReason { get; set; }
    public required DateTime DateSubmitted { get; set; }
    public required ReviewStatusEnum Status { get; set; }
    public Guid AccountId { get; set; }
    public AccountEntity Account { get; set; } = null!;

}
