namespace NettoRetail.Application.Models.Request.Customer
{
    public class ResetCustomerPasswordRequest
    {
        public string Token { get; set; }
        public string Password { get; set; }
        public string PasswordConfirmation { get; set; }
    }
}
