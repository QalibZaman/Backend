using Microsoft.EntityFrameworkCore;
using NettoRetail.DataAccess.Context;
using NettoRetail.Domain.Entities;
using NettoRetail.Domain.Enums;
using NettoRetail.Shared.Interfaces.Repositories;

namespace NettoRetail.DataAccess.Repositories;

public class ReviewRepository : BaseRepository<ReviewEntity>, IReviewRepository
{
    public ReviewRepository(AppDbContext dbContext) : base(dbContext)
    {
    }
}
