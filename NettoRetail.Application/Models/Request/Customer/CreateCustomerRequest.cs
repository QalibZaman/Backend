namespace NettoRetail.Application.Models.Request.Customer;

public class CreateCustomerRequest
{
    public required string FirstName { get; set; }
    public required string LastName { get; set; }
    public required string Email { get; set; }
    public int CountryId { get; set; }
    public string? PhoneNumber { get; set; }
    public string? Remarks { get; set; }
}
