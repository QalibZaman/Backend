using NettoRetail.Domain.Entities;
using NettoRetail.Shared.Specifications;

namespace NettoRetail.DataAccess.Specifications.Customer;

public class GetCustomerByIdSpecification : Specification<CustomerEntity>
{
    public GetCustomerByIdSpecification(Guid id)
        : base(customer => customer.Id == id)
    {
        AddInclude(x => x.Shop!);
    }
}