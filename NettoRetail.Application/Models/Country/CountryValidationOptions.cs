namespace NettoRetail.Application.Models.Country;

public class CountryValidationOptions
{
    public bool CheckForPhoneNumber { get; set; } = true;
    public bool CheckForZipCode { get; set; } = true;
}