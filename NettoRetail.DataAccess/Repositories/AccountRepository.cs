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
}