namespace NettoRetail.Application.Models.Response;

public class CountryResponse
{
    public int Id { get; set; }
    public required string Name { get; set; }
    public required string PhoneCode { get; set; }
    public required string PhoneMask { get; set; }
    public required string ZipCodeMask { get; set; }
}
