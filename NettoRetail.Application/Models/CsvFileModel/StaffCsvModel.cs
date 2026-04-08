using CsvHelper.Configuration.Attributes;

namespace NettoRetail.Application.Models.CsvFileModel;

public class StaffCsvModel
{
    [Name("First Name")]
    public required string FirstName { get; set; }

    [Name("Last Name")]
    public required string LastName { get; set; }

    [Name("Email")]
    public required string Email { get; set; }

    [Name("Role")]
    public required string Role { get; set; }

    [Name("Status")]
    public string? Status { get; set; }
}
