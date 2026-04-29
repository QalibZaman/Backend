using NettoRetail.Domain.Entities;
using NettoRetail.Shared.Specifications;

namespace NettoRetail.DataAccess.Specifications.Customer;

public class GetCustomerByLoyaltyIdWithOrdersSpecification : Specification<CustomerEntity>
{
    public GetCustomerByLoyaltyIdWithOrdersSpecification(string loyaltyId)
        : base(customer => customer.LoyaltyId != null && customer.LoyaltyId.ToLower() == loyaltyId.ToLower())
    {
        AddInclude(x => x.Order!);
    }
}
