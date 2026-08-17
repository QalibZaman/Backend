namespace NettoRetail.Application.Models.Response
{
    public class AccountDetailsResponse
    {
        public Guid Id { get; set; }
        public required string FirstName { get; set; }
        public required string LastName { get; set; }
        public required string Email { get; set; }
        public required int RoleId { get; set; }
        public required string Status { get; set; }
        public required bool IsOnboardingCompleted { get; set; }
        public string? PhoneNumber { get; set; }
        public string? ShippingAddress { get; set; }
        public string? City { get; set; }
        public string? State { get; set; }
        public string? ZipCode { get; set; }
        public int? CountryId { get; set; }
        public string? ProfileImageUrl { get; set; }
        public int? ShopId { get; set; }
    }

}
