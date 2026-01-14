namespace NettoRetail.Application.Models.Shop;

public class ShopModel
{
    public required string Name { get; set; }
    public required string Email { get; set; }
    public required string PhoneNumber { get; set; }
    public required string Url { get; set; }
    public string? Address { get; set; }
    public string? City { get; set; }
    public string? State { get; set; }
    public string? ZipCode { get; set; }
    public string? Description { get; set; }
    public int? CountryId { get; set; }
}
