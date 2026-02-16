using NettoRetail.Domain.Entities;
using NettoRetail.Shared.Specifications;

namespace NettoRetail.DataAccess.Specifications.Customer;

public sealed class GetCustomerByRemarksSpecification : Specification<CustomerEntity>
{
    public GetCustomerByRemarksSpecification(string remarks)
        : base(c => c.Remarks != null && c.Remarks.Contains(remarks))
    {
    }
}
