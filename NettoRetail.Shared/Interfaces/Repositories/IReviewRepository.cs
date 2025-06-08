using NettoRetail.Domain.Entities;
using NettoRetail.Domain.Enums;

namespace NettoRetail.Shared.Interfaces.Repositories;

public interface IReviewRepository : IBaseRepository<ReviewEntity>
{
    Task<List<ReviewEntity>> GetFilteredReviewsAsync(string? shopName, int? rating, ReviewStatusEnum? status, DateRangeFilterEnum dateRange, CancellationToken cancellationToken);
}
