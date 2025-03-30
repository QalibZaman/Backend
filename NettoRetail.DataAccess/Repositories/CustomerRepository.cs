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

    public async Task<ICollection<CustomerEntity>> GetCustomersPaginatedAsync(Specification<CustomerEntity> specification, CancellationToken cancellationToken)
    {
        var response = _dbSet.Where(specification.Criteria)
            .ApplyPagination(specification);

        foreach (var include in specification.Includes)
        {
            response = response.Include(include);
        }

        return await response.ToListAsync(cancellationToken);
    }
}
