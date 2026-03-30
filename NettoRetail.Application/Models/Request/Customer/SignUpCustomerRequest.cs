namespace NettoRetail.Application.Models.Request.Customer;

public class SignUpCustomerRequest
{
    public required string FirstName { get; set; }
    public required string LastName { get; set; }
    public required string Email { get; set; }
    public int CountryId { get; set; }
    public required string PhoneNumber { get; set; }
    public required string Password { get; set; }
    public required string ConfirmPassword { get; set; }
    public bool PrivacyPolicyAccepted { get; set; }
    public string? Remarks { get; set; } = "";
}
