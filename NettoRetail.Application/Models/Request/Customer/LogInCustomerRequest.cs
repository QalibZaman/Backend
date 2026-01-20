using System.ComponentModel.DataAnnotations;

namespace NettoRetail.Application.Models.Request.Customer
{
    public class LogInCustomerRequest
    {
        [Required]
        [EmailAddress]
        public required string Email { get; set; }
        [Required]
        public required string Password { get; set; }
        public bool RememberMe { get; set; }
    }
}
