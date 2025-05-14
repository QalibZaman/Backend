using System.Linq;
using Microsoft.EntityFrameworkCore;
using NettoRetail.DataAccess.Context;
using NettoRetail.Domain.Entities;
using NettoRetail.Shared.Extensions;
using NettoRetail.Shared.Interfaces.Repositories;
using NettoRetail.Shared.Specifications;
using static Microsoft.EntityFrameworkCore.DbLoggerCategory;

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

    public async Task<IEnumerable<AccountEntity>> GetStaffAccounts(Specification<AccountEntity> specification, CancellationToken cancellationToken)
    {
        var accountsWithoutCustomers = _dbSet.Where(specification.Criteria);

        foreach (var include in specification.Includes)
        {
            accountsWithoutCustomers = accountsWithoutCustomers.Include(include);
        }

        accountsWithoutCustomers = accountsWithoutCustomers
            .GroupJoin(
                _dbContext.Customers,
                account => account.Id,
                customer => customer.AccountId,
                (account, customers) => new { account, customers })
            .SelectMany(
                ac => ac.customers.DefaultIfEmpty(),
                (ac, customer) => new { ac.account, customer })
            .Where(x => x.customer == null)
            .Select(x => x.account);

        return await accountsWithoutCustomers.ToListAsync(cancellationToken);
    }

    public async Task<(ICollection<AccountEntity> Items, int TotalCount)> GetStaffAccountsPaginated(
        Specification<AccountEntity> specification,
        CancellationToken cancellationToken)
    {
        var query = _dbSet.AsQueryable();

        if (specification.Criteria != null)
        {
            query = query.Where(specification.Criteria);
        }

        query = query
            .GroupJoin(
                _dbContext.Customers,
                account => account.Id,
                customer => customer.AccountId,
                (account, customers) => new { account, customers })
            .SelectMany(
                ac => ac.customers.DefaultIfEmpty(),
                (ac, customer) => new { ac.account, customer })
            .Where(x => x.customer == null)
            .Select(x => x.account);

        foreach (var include in specification.Includes)
        {
            query = query.Include(include);
        }

        if (specification.Orderings != null && specification.Orderings.Any())
        {
            var (expr, isDesc) = specification.Orderings.First();
            var orderedQuery = isDesc
                ? query.OrderByDescending(expr)
                : query.OrderBy(expr);

            foreach (var ordering in specification.Orderings.Skip(1))
            {
                orderedQuery = ordering.IsDesc
                    ? orderedQuery.ThenByDescending(ordering.Expr)
                    : orderedQuery.ThenBy(ordering.Expr);
            }

            query = orderedQuery;
        }

        var totalCount = await query.CountAsync(cancellationToken);

        query = query.ApplyPagination(specification);

        var items = await query.ToListAsync(cancellationToken);

        return (items, totalCount);
    }
}
