using Microsoft.EntityFrameworkCore;
using NettoRetail.Domain.Entities;
using NettoRetail.Shared.Specifications;

namespace NettoRetail.DataAccess.Specifications.Order;

public class GetOrderSearchSpecification : Specification<OrderEntity>
{
    public GetOrderSearchSpecification(string searchTerm)
        : base(x => (
                    EF.Functions.Like(x.Id.ToString(), $"%{searchTerm}%") ||
                    EF.Functions.Like(x.Customer.Account.Email.ToLower(), $"%{searchTerm}%") ||
                    EF.Functions.Like(x.CustomerId.ToString(), $"%{searchTerm}%") ||
                    EF.Functions.Like(x.Customer.Account.FirstName.ToLower(), $"%{searchTerm}%") ||
                    EF.Functions.Like(x.Customer.Account.LastName.ToLower(), $"%{searchTerm}%") ||
                    EF.Functions.Like(x.OrderStatus.ToString().ToLower(), $"%{searchTerm}%") ||
                    EF.Functions.Like(x.DeliveryType.ToString().ToLower(), $"%{searchTerm}%") ||
                    EF.Functions.Like(x.CreationDate.ToString().ToLower(), $"%{searchTerm}%") ||
                    EF.Functions.Like(x.Creator.ToLower(), $"%{searchTerm}%")))
    {
        searchTerm = searchTerm.ToLower();
    }
}
