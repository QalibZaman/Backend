using System.Linq.Expressions;
using NettoRetail.Domain.Entities;
using NettoRetail.Shared.Specifications;

namespace NettoRetail.DataAccess.Specifications.Customer;

public sealed class GetCustomerBySearchSpecification(string? search) : Specification<CustomerEntity>(BuildCriteria(search))
{
    private static Expression<Func<CustomerEntity, bool>> BuildCriteria(string? rawSearch)
    {
        if (string.IsNullOrWhiteSpace(rawSearch))
            return c => true;

        var term = rawSearch.Trim().ToLower();

        return c =>
            (c.Account.FirstName != null && c.Account.FirstName.ToLower().Contains(term)) ||
            (c.Account.LastName != null && c.Account.LastName.ToLower().Contains(term)) ||
            (((c.Account.FirstName ?? string.Empty) + " " + (c.Account.LastName ?? string.Empty)).ToLower().Contains(term)) ||
            (c.Account.Email != null && c.Account.Email.ToLower().Contains(term)) ||
            (c.LoyaltyId != null && c.LoyaltyId.ToLower().Contains(term));
    }
}
