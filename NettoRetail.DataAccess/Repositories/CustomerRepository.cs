using Microsoft.EntityFrameworkCore;
using NettoRetail.DataAccess.Context;
using NettoRetail.Domain.Entities;
using NettoRetail.Shared.Extensions;
using NettoRetail.Shared.Interfaces.Repositories;
using NettoRetail.Shared.Specifications;

namespace NettoRetail.DataAccess.Repositories;

public class CustomerRepository : BaseRepository<CustomerEntity>, ICustomerRepository
{
    public CustomerRepository(AppDbContext dbContext)
        : base(dbContext) { }

    public async Task<(ICollection<CustomerEntity> Items, int TotalAmount)> GetCustomersPaginatedAsync(Specification<CustomerEntity> specification, CancellationToken cancellationToken)
    {
        var query = _dbSet.Where(specification.Criteria);

        foreach (var include in specification.Includes)
        {
            query = query.Include(include);
        }

        var totalAmount = await query.CountAsync(cancellationToken);
        var response = query.ApplyPagination(specification);

        var items = await response.ToListAsync();
        return (items, totalAmount);
    }
}
