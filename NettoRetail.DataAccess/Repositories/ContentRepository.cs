using NettoRetail.DataAccess.Context;
using NettoRetail.Domain.Entities;
using NettoRetail.Shared.Interfaces.Repositories;

namespace NettoRetail.DataAccess.Repositories;

public class ContentRepository : BaseRepository<ContentEntity>, IContentRepository
{
    public ContentRepository(AppDbContext context) : base(context)
    {
    }
}
