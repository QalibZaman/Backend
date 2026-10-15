namespace NettoRetail.Domain.Entities
{
    public class AddressEntity
    {
        public Guid Id { get; set; }
        public Guid? CustomerId { get; set; }
        public CustomerEntity? Customer { get; set; }
        public Guid? StaffId { get; set; }
        public StaffEntity? Staff { get; set; }
        public int? CountryId { get; set; }
        public CountryEntity? Country { get; set; }
        public string? Street { get; set; }
        public string? HouseNumber { get; set; }
        public string? Apartment { get; set; }
        public string? City { get; set; }
        public string? State { get; set; }
        public string? ZipCode { get; set; }
    }
}
