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
            (c.FirstName != null && c.FirstName.ToLower().Contains(term)) ||
            (c.LastName != null && c.LastName.ToLower().Contains(term)) ||
            (((c.FirstName ?? string.Empty) + " " + (c.LastName ?? string.Empty)).ToLower().Contains(term)) ||
            (c.Email != null && c.Email.ToLower().Contains(term)) ||
            (c.LoyaltyId != null && c.LoyaltyId.ToLower().Contains(term));
    }
}
