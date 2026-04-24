using NettoRetail.Application.Models.Response.Country;

namespace NettoRetail.Application.Models.Response.Address;

public class AddressResponse
{
    public Guid Id { get; set; }
    public string? ZipCode { get; set; }
    public string? City { get; set; }
    public string? Street { get; set; }
    public string? BuildingNumber { get; set; }
    public int Floor { get; set; }
    public int Entrance { get; set; }
    public int? OfficeNumber { get; set; }
    public int? ApartmentNumber { get; set; }
    public CountryLookupDTO Country { get; set; }
}
