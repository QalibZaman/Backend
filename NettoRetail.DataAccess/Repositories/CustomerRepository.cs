using Microsoft.EntityFrameworkCore;
using NettoRetail.DataAccess.Context;
using NettoRetail.Domain.Entities;
using NettoRetail.Shared.Constants;
using NettoRetail.Shared.Interfaces.Repositories;

namespace NettoRetail.DataAccess.Repositories;

public class CustomerRepository(AppDbContext dbContext) : BaseRepository<CustomerEntity>(dbContext), ICustomerRepository
{
    public async Task<bool> ExistsActiveCustomerByAccountEmailAsync(string email, CancellationToken cancellationToken)
    {
        return await (
            from c in _dbContext.Customers
            join a in _dbContext.Accounts on c.AccountId equals a.Id
            where c.IsActive && a.Email == email
                && a.RoleId == RoleConstants.Customer
            select 1)
        .AnyAsync(cancellationToken);
    }
}
