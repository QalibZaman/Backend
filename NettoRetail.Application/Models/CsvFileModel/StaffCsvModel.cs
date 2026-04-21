using System.Text.RegularExpressions;
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
    [Name("Staff Member Status")]
    public string? Status { get; set; }
    public static bool IsValid(StaffCsvModel model)
    {
        var namePattern = new Regex(@"^([A-Z][a-z]*)( [A-Z][a-z]*)*$");
        var emailPattern = new Regex(@"^[A-Za-z0-9._%+\-]+@[A-Za-z0-9.\-]+\.[A-Za-z]{2,}$");

        // Firstname validation
        if (model.FirstName == null) return false;
        if (model.FirstName.Length < 2 || model.FirstName.Length > 30)
            return false;
        if (!namePattern.IsMatch(model.FirstName))
            return false;

        // Lastname validation
        if (model.LastName == null) return false;
        if (model.LastName.Length < 2 || model.LastName.Length > 30)
            return false;
        if (!namePattern.IsMatch(model.LastName))
            return false;

        // Email validation
        if (model.Email == null) return false;
        if (model.Email.Length > 256) return false;
        if (!emailPattern.IsMatch(model.Email)) return false;

        // Role validation
        if (Convert.ToInt32(model.Role) <= 0) return false;

        return true;
    }

    public static bool IsCollectionValid(IEnumerable<StaffCsvModel> models)
    {
        foreach (var model in models)
        {
            if (!IsValid(model)) return false;
        }

        return true;
    }
}
