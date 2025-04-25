using Microsoft.EntityFrameworkCore;
using NettoRetail.Domain.Entities;
using NettoRetail.Shared.Specifications;

public class GetOrderSearchSpecification : Specification<OrderEntity>
{
    public GetOrderSearchSpecification(string searchTerm, int shopId)
        : base(x => x.ShopId == shopId &&
            (
                x.Id.ToString().Contains(searchTerm) ||
                x.Customer.Account.Email.ToLower().Contains(searchTerm)
            ))
    {
        searchTerm = searchTerm.ToLower();
    }
}
