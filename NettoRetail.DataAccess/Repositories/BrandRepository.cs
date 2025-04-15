using NettoRetail.DataAccess.Context;
using NettoRetail.Domain.Entities;
using NettoRetail.Shared.Interfaces.Repositories;

namespace NettoRetail.DataAccess.Repositories;

public class BrandRepository : BaseRepository<BrandEntity>, IBrandRepository
{
    public BrandRepository(AppDbContext dbContext)
    : base(dbContext) { }
}
