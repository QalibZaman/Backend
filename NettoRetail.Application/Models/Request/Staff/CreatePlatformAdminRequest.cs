namespace NettoRetail.Application.Models.Request.Staff;

public class CreatePlatformAdminRequest
{
    public string FirstName { get; set; } = default!;
    public string LastName { get; set; } = default!;
    public string Email { get; set; } = default!;
    public string? PhoneNumber { get; set; }
    public List<string> Permissions { get; set; } = new();
}