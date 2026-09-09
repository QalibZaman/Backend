using NettoRetail.DataAccess.Context;
using NettoRetail.Domain.Entities;
using NettoRetail.Shared.Interfaces.Repositories;

namespace NettoRetail.DataAccess.Repositories;

public class CategoryRepository(AppDbContext dbContext)
    : BaseRepository<CategoryEntity>(dbContext), ICategoryRepository
{
}