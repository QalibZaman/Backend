namespace NettoRetail.Application.Models.Response;

public class CreateStaffResponse
{
    public Guid Id { get; set; }
    public required string FirstName { get; set; }
    public required string LastName { get; set; }
    public required string Email { get; set; }
    public required int RoleId { get; set; }
}
