namespace NettoRetail.Application.Models.Request.Customer;

public record VerifyCustomerEmailRequest(string Token, string Email);
