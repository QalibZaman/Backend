namespace NettoRetail.Application.Models.Request.Staff;

public class CreatePlatformAdminRequest
{
    public required string FirstName { get; set; } = default!;
    public required string LastName { get; set; } = default!;
    public required string Email { get; set; } = default!;
    public string? PhoneNumber { get; set; }
    public required PlatformAdminPermissionRequest Permissions { get; init; }
        = new PlatformAdminPermissionRequest();

}