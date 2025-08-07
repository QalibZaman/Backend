using NettoRetail.DataAccess.Context;
using NettoRetail.DataAccess.Repositories;
using NettoRetail.Domain.Entities;

public class TeaserSectionRepository : BaseRepository<TeaserSectionEntity>, ITeaserSectionRepository
{
    public TeaserSectionRepository(AppDbContext dbContext) : base(dbContext) { }
}
