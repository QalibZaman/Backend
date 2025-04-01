namespace NettoRetail.Application.Models.Request.Staff;
public class CreateStaffRequest
{
    public required string FirstName { get; set; }
    public required string LastName { get; set; }
    public required string Email { get; set; }
    public required int RoleId { get; set; }
}
