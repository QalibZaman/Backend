namespace NettoRetail.Application.Models.Request.Customer
{
    public class LogInCustomerRequest
    {
        public required string Email { get; set; }
        public required string Password { get; set; }
        public bool RememberMe { get; set; }
    }
}
