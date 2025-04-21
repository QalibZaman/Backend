using NettoRetail.Domain.Entities;
using NettoRetail.Shared.Specifications;

namespace NettoRetail.DataAccess.Specifications.Order;

public class GetOrdersByEmailsSpecification : Specification<OrderEntity>
{
    public GetOrdersByEmailsSpecification(List<string> emails)
        : base(x => emails.Contains(x.Customer.Account.Email))
    {
        AddInclude(x => x.Customer.Account);
    }
}