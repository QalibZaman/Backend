using NettoRetail.Domain.Entities;
using NettoRetail.Shared.Specifications;

namespace NettoRetail.DataAccess.Specifications.Shop;

public class GetShopByPhoneNumberSpecification : Specification<ShopEntity>
{
    public GetShopByPhoneNumberSpecification(string phoneNumber)
    : base(x => x.PhoneNumber == phoneNumber) { }
}
