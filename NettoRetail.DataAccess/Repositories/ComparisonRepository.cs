using NettoRetail.DataAccess.Context;
using NettoRetail.Domain.Entities;
using NettoRetail.Shared.Interfaces.Repositories;

namespace NettoRetail.DataAccess.Repositories
{
    public class ComparisonRepository(AppDbContext dbContext) : BaseRepository<ComparisonListEntity>(dbContext), IComparisonRepository { }
}
