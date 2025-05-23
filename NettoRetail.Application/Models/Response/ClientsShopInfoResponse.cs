using NettoRetail.Domain.Enums;

namespace NettoRetail.Application.Models.Response;

public class ClientsShopInfoResponse
{
    public int Id { get; set; }
    public required string Name { get; set; }
    public required string AdminsFullName { get; set; }
    public required string AdminsEmail { get; set; }
    public required string ShopEmail { get; set; }
    public required string PhoneNumber { get; set; }
    public required string Url { get; set; }
    public required string Address { get; set; }
    public required string Country { get; set; }
    public required string City { get; set; }
    public required string State { get; set; }
    public required string ZipCode { get; set; }
    public required string Description { get; set; }
    public required string Subscription { get; set; }
}