using NettoRetail.Domain.Entities;
using NettoRetail.Shared.Specifications;

namespace NettoRetail.DataAccess.Specifications.Order;

public class GetOrdersByEmailsSpecification : Specification<OrderEntity>
{
    public GetOrdersByEmailsSpecification(List<string> emails)
        : base(x => emails
            .Where(email => !string.IsNullOrWhiteSpace(email))
            .Select(email => email.Trim().ToLower())
            .Any(email => x.Customer.Email.ToLower().Contains(email)))
    {
        AddInclude(x => x.Customer);
    }
}