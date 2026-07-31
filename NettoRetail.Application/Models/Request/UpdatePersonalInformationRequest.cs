namespace NettoRetail.Application.Models.Request
{
    public class UpdatePersonalInformationRequest
    {
        public required string FirstName { get; set; }
        public required string LastName { get; set; }
        public required string Email { get; set; }
        public string? PhoneNumber { get; set; }
        public int? CountryId { get; set; }
        public string? Street { get; set; }
        public string? HouseNumber { get; set; }
        public string? Apartment { get; set; }
        public string? City { get; set; }
        public string? State { get; set; }
        public string? ZipCode { get; set; }
    }
}
