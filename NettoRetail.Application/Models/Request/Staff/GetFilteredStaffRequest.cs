namespace NettoRetail.Application.Models.Request.Staff;

public class GetFilteredStaffRequest
{
    public string? FirstName { get; set; }
    public string? LastName { get; set; }
    public string? Email { get; set; }
    public int? RoleId { get; set; }
    public string? Status { get; set; }

    public List<SortingOption>? SortingOptions { get; set; }
}
