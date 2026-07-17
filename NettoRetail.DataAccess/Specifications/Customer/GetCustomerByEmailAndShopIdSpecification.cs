using NettoRetail.Domain.Entities;
using NettoRetail.Shared.Specifications;

namespace NettoRetail.DataAccess.Specifications.Customer;

public class GetCustomerByEmailAndShopIdSpecification : Specification<CustomerEntity>
{
    public GetCustomerByEmailAndShopIdSpecification(string email, int shopId)
        : base(c => c.Email == email) { }
}