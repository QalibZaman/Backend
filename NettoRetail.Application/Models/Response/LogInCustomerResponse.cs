namespace NettoRetail.Application.Models.Response
{
    public class LogInCustomerResponse
    {
        public required string AccessToken { get; set; }
        public string? RefreshToken { get; set; }
        public DateTimeOffset AccessTokenExpiresAt { get; set; }

        public Guid AccountId { get; set; }
        public int ShopId { get; set; }
        public int RoleId { get; set; }

        public string? FirstName { get; set; }
        public string? LastName { get; set; }
        public string? Email { get; set; }
    }
}
