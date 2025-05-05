namespace NettoRetail.Application.Models.ClientsShop;

public class ClientsShopModel
{
    public required string Name { get; set; }
    public required Guid AccountId { get; set; }
    public required string Email { get; set; }
    public required string PhoneNumber { get; set; }
    public required string Url { get; set; }
    public required string Address { get; set; }
    public required string City { get; set; }
    public required string State { get; set; }
    public required string ZipCode { get; set; }
    public required string Description { get; set; }
    public int CountryId { get; set; }
}