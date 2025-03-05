using Mapster;
using Microsoft.EntityFrameworkCore;
using NettoRetail.DataAccess.Context;
using NettoRetail.Domain.Entities;
using NettoRetail.Shared.Interfaces.Repositories;
using NettoRetail.Shared.Specifications;

namespace NettoRetail.DataAccess.Repositories;

public class CategoryRepository(AppDbContext dbContext)
    : BaseRepository<CategoryEntity>(dbContext), ICategoryRepository
{
    public async Task<ICollection<CategoryEntity>> GetAllNested(Specification<CategoryEntity> specification, CancellationToken cancellationToken)
    {
        var response = await _dbSet.Where(specification.Criteria)
             .ToListAsync(cancellationToken);

        return response;
    }
}
