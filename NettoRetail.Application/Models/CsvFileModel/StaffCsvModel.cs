using CsvHelper.Configuration.Attributes;
using Mapster;
using Microsoft.AspNetCore.Mvc;
using NettoRetail.Application.Models.Request.Staff;

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
    public string? Status { get; set; }
    public static List<CreateStaffRequest> AdaptCsvModelToCreateStaffRequest(ICollection<StaffCsvModel> csvModels)
    {
        List<CreateStaffRequest> result = new(capacity: csvModels.Count);
        foreach (var csvModel in csvModels)
        {
                var staffRequest = TypeAdapter.ValidateAndAdapt<StaffCsvModel, CreateStaffRequest>(csvModel);
                result.Add(staffRequest);
        }

        return result;
    }
}
