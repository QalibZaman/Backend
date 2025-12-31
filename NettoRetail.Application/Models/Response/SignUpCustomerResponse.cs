namespace NettoRetail.Application.Models.Response;

public class SignUpCustomerResponse
{
    public required string FirstName { get; set; }
    public required string LastName { get; set; }
    public required string Email { get; set; }
    public required bool IsActive { get; set; }
    public string? ActivationLink { get; set; }
}
