namespace NettoRetail.Domain.Entities;

public class CountryEntity
{
    public int Id { get; set; }
    public required string Name { get; set; }
    public required string PhoneCode { get; set; }
    public required string PhoneMask { get; set; }
    public required string ZipCodeMask { get; set; }
    public ICollection<ShopEntity>? Shops { get; set; }
    public ICollection<BaseUserEntity>? Accounts { get; set; }
    public ICollection<AddressEntity>? Addresses { get; set; }
}
