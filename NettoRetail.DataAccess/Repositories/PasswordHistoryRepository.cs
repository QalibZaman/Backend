using NettoRetail.DataAccess.Context;
using NettoRetail.Domain.Entities;
using NettoRetail.Shared.Interfaces.Repositories;

namespace NettoRetail.DataAccess.Repositories
{
    public class PasswordHistoryRepository : BaseRepository<PasswordHistoryEntity>, IPasswordHistoryRepository
    {
        public PasswordHistoryRepository(AppDbContext dbContext)
        : base(dbContext) { }
    }
}
