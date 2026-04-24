namespace NettoRetail.Application.Models.Request.Address;

public class CreateAddressRequest
{
    public required int CountryId { get; set; }
    public required string ZipCode { get; set; }
    public required string City { get; set; }
    public required string Street { get; set; }
    public required string BuildingNumber { get; set; }
    public int Floor { get; set; }
    public int Entrance { get; set; }
    public int? OfficeNumber { get; set; }
    public int? ApartmentNumber { get; set; }
}
