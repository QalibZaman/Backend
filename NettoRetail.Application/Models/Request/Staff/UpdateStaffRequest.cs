namespace NettoRetail.Application.Models.Request.Staff;
public class UpdateStaffRequest
{
    public string? FirstName { get; set; }
    public string? LastName { get; set; }
    public string? Email { get; set; }
    public int? RoleId { get; set; }
    public bool? IsActive { get; set; }
}
