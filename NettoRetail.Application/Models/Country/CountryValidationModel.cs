namespace NettoRetail.Application.Models.Country;

public class CountryValidationModel
{
    public required int CountryId { get; set; }
    public string PhoneNumber { get; set; } = string.Empty;
    public string ZipCode { get; set; } = string.Empty;
}