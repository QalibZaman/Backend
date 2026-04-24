namespace NettoRetail.Domain.Entities;

public class AddressEntity
{
    public Guid Id { get; set; }
    public BaseUserEntity User { get; set; }
    public Guid UserId { get; set; }
    public CountryEntity Country { get; set; }
    public int CountryId { get; set; }
    public string ZipCode { get; set; }
    public string City { get; set; }
    public string Street { get; set; }
    public string BuildingNumber { get; set; }
    public int Floor { get; set; }
    public int Entrance { get; set; }
    public int? OfficeNumber { get; set; }
    public int? ApartmentNumber { get; set; }
    public bool IsDefault { get; set; }
}
