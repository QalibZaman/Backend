using Microsoft.EntityFrameworkCore;
using NettoRetail.DataAccess.Context;
using NettoRetail.Domain.Entities;
using NettoRetail.Domain.Enums;
using NettoRetail.Shared.Interfaces.Repositories;

namespace NettoRetail.DataAccess.Repositories;

public class ReviewRepository : BaseRepository<ReviewEntity>, IReviewRepository
{
    private readonly AppDbContext dbContext;

    public ReviewRepository(AppDbContext dbContext) : base(dbContext)
    {
        this.dbContext = dbContext;
    }

    public async Task<List<ReviewEntity>> GetFilteredReviewsAsync(string? shopName, int? rating, ReviewStatusEnum? status, DateRangeFilterEnum dateRange, CancellationToken cancellationToken)
    {
        var query = dbContext.Reviews.AsQueryable();

        if (!string.IsNullOrEmpty(shopName))
            query = query.Where(r => r.ShopName.Contains(shopName));

        if (rating.HasValue)
            query = query.Where(r => r.Rating == rating.Value);

        if (status.HasValue)
            query = query.Where(r => r.Status == status.Value);

        if (dateRange != DateRangeFilterEnum.All)
        {
            var dateFilter = dateRange switch
            {
                DateRangeFilterEnum.Today => DateTime.UtcNow.Date,
                DateRangeFilterEnum.Last7Days => DateTime.UtcNow.AddDays(-7),
                DateRangeFilterEnum.LastMonth => DateTime.UtcNow.AddMonths(-1),
                _ => (DateTime?)null
            };

            if (dateFilter.HasValue)
                query = query.Where(r => r.DateSubmitted >= dateFilter.Value);
        }

        return await query.ToListAsync(cancellationToken);
    }
}
