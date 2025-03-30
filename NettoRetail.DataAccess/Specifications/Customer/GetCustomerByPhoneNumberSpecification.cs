using NettoRetail.Domain.Entities;
using NettoRetail.Shared.Specifications;

namespace NettoRetail.DataAccess.Specifications.Customer;

public class GetCustomerByPhoneNumberSpecification : Specification<CustomerEntity>
{
    public GetCustomerByPhoneNumberSpecification(string phoneNumber)
        : base(customer => customer.PhoneNumber != null && customer.PhoneNumber.Contains(phoneNumber)) { }
}
