using NettoRetail.Domain.Entities;
using NettoRetail.Shared.Specifications;

namespace NettoRetail.DataAccess.Specifications.Customer;

public class GetCustomerByLoyaltyIdSpecification : Specification<CustomerEntity>
{
    public GetCustomerByLoyaltyIdSpecification(string loyaltyId)
        : base(customer => customer.LoyaltyId != null && customer.LoyaltyId.ToLower() == loyaltyId.ToLower())
    { }
}
