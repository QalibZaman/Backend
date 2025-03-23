namespace NettoRetail.Application.Models.Country;

public class CountryValidationModel
{
    public required int CountryId { get; set; }
    public required string PhoneNumber { get; set; }
    public required string ZipCode { get; set; }
}
