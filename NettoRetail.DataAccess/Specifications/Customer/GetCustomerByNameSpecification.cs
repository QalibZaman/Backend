using System.Linq.Expressions;
using NettoRetail.Domain.Entities;
using NettoRetail.Shared.Specifications;

namespace NettoRetail.DataAccess.Specifications.Customer;

public sealed class GetCustomerByNameSpecification(string? name) : Specification<CustomerEntity>(BuildCriteria(name))
{
    private static Expression<Func<CustomerEntity, bool>> BuildCriteria(string? rawName)
    {
        if (string.IsNullOrWhiteSpace(rawName))
            return c => true;

        var parts = rawName
            .Trim()
            .ToLower()
            .Split(' ', StringSplitOptions.RemoveEmptyEntries);

        if (parts.Length == 0)
            return c => true;

        if (parts.Length == 1)
        {
            var term = parts[0];

            return c =>
                c.Account.FirstName.ToLower().Contains(term) ||
                c.Account.LastName.ToLower().Contains(term);
        }

        var firstPart = parts[0];
        var secondPart = parts[1];

        return c =>
            (c.Account.FirstName.ToLower().Contains(firstPart) ||
             c.Account.LastName.ToLower().Contains(firstPart)) &&
            (c.Account.FirstName.ToLower().Contains(secondPart) ||
             c.Account.LastName.ToLower().Contains(secondPart));
    }
}
