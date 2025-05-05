using Microsoft.EntityFrameworkCore;
using NettoRetail.DataAccess.Context;
using NettoRetail.Domain.Entities;
using NettoRetail.Shared.Interfaces.Repositories;
using NettoRetail.Shared.Specifications;

namespace NettoRetail.DataAccess.Repositories;

public class AccountRepository : BaseRepository<AccountEntity>,
    IAccountRepository
{
    public AccountRepository(AppDbContext dbContext)
        : base(dbContext)
    { }

    public async Task<IEnumerable<string>> GetAccountEmailsAsync(Specification<AccountEntity> specification, CancellationToken cancellationToken)
    {
        var emails = await _dbSet.Where(specification.Criteria).Select(x => x.Email).ToListAsync(cancellationToken);

        return emails;
    }

    public async Task<IEnumerable<AccountEntity>> GetStaffAccounts(int shopId, CancellationToken cancellationToken)
    {
        var accountsWithoutCustomers = await _dbSet.Where(a => a.ShopId == shopId && !_dbContext.Customers.Any(c => c.AccountId == a.Id)).Include(a => a.Role)
            .ToListAsync(cancellationToken);

        return accountsWithoutCustomers;
    }
}