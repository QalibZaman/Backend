using NettoRetail.Domain.Entities;
using NettoRetail.Shared.Specifications;

namespace NettoRetail.DataAccess.Specifications.Customer;

public sealed class GetCustomerByPhoneNumberSpecification : Specification<CustomerEntity>
{
    public GetCustomerByPhoneNumberSpecification(string phoneNumber)
        : base(c => c.PhoneNumber != null && c.PhoneNumber.Contains(phoneNumber))
    {
    }
}
