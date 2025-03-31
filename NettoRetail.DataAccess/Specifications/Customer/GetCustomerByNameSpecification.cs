using NettoRetail.Domain.Entities;
using NettoRetail.Shared.Specifications;

namespace NettoRetail.DataAccess.Specifications.Customer;

public class GetCustomerByNameSpecification : Specification<CustomerEntity>
{
    public GetCustomerByNameSpecification(string name)
        : base(customer => (customer.Account.FirstName + " " + customer.Account.LastName).ToLower()
            .Contains(name.ToLower())) { }
}
