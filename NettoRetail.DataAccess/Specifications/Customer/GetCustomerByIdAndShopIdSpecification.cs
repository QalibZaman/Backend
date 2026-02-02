using NettoRetail.Domain.Entities;
using NettoRetail.Shared.Specifications;

namespace NettoRetail.DataAccess.Specifications.Customer;

public class GetCustomerByIdAndShopIdSpecification:Specification<CustomerEntity>
{
    public GetCustomerByIdAndShopIdSpecification(Guid id,int shopId)
        : base(customer => customer.AccountId == id && customer.Account.ShopId == shopId)
    {
        AddInclude(x => x.Account);
    }
}