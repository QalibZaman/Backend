using NettoRetail.Domain.Entities;
using NettoRetail.Shared.Specifications;

namespace NettoRetail.DataAccess.Specifications.Customer;

public class GetCustomerByEmailSpecification : Specification<CustomerEntity>
{
    public GetCustomerByEmailSpecification(string email)
        : base(customer => customer.Account.Email.Contains(email)) { }
}