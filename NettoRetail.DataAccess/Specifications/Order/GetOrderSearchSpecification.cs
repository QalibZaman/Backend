using Microsoft.EntityFrameworkCore;
using NettoRetail.Domain.Entities;
using NettoRetail.Shared.Specifications;

public class GetOrderSearchSpecification : Specification<OrderEntity>
{
    public GetOrderSearchSpecification(string searchTerm)
        : base(x => (
                EF.Functions.Like(x.Id.ToString(), searchTerm) ||
                EF.Functions.Like(x.Customer.Account.Email.ToLower().ToString(), searchTerm) ||
                EF.Functions.Like(x.Customer.Account.FirstName, searchTerm) ||
                EF.Functions.Like(x.Customer.Account.LastName, searchTerm) ||
                EF.Functions.Like(x.CreationDate.ToString(), searchTerm) ||
                EF.Functions.Like(x.OrderStatus.ToString(), searchTerm) ||
                EF.Functions.Like(x.DeliveryType.ToString(), searchTerm) ||
                EF.Functions.Like(x.Creator, searchTerm)))
    {
        searchTerm = searchTerm.ToLower();
    }
}
