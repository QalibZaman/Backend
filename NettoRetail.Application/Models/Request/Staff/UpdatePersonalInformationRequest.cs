namespace NettoRetail.Application.Models.Request.Staff;

public class UpdatePersonalInformationRequest
{
    public required string FirstName { get; set; }
    public required string LastName { get; set; }
    public required string Email { get; set; }
    public string? PhoneNumber { get; set; }
    public string? ShippingAddress { get; set; }
    public string? City { get; set; }
    public string? State { get; set; }
    public string? ZipCode { get; set; }
    public int? CountryId { get; set; }
}
