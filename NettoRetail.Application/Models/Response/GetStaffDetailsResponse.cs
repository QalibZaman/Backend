namespace NettoRetail.Application.Models.Response;

public class StaffDetailsResponse
{
    public Guid Id { get; set; }
    public required string FirstName { get; set; }
    public required string LastName { get; set; }
    public required string Email { get; set; }
    public required int RoleId { get; set; }
    public required string Status { get; set; }
    public DateTime? PasswordUpdatedAt { get; set; }
}
