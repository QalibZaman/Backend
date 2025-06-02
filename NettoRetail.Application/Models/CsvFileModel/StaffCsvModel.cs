using CsvHelper.Configuration.Attributes;
using NettoRetail.Domain.Enums;

namespace NettoRetail.Application.Models.CsvFileModel;

public class StaffCsvModel
{
    [Name("First name")]
    public required string FirstName { get; set; }
    [Name("Last name")]
    public required string LastName { get; set; }
    [Name("Email")]
    public required string Email { get; set; }
    [Name("Role")]
    public required string Role { get; set; }
    [Name("Status")]
    public required string Status { get; set; }
    [Name("Password")]
    public required string Password { get; set; }
}
