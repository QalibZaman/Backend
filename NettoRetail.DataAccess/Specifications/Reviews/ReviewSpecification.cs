using Microsoft.Azure.Cosmos.Linq;
using NettoRetail.Domain.Entities;
using NettoRetail.Domain.Enums;
using NettoRetail.Shared.Specifications;

namespace NettoRetail.DataAccess.Specifications.Reviews;

public class ReviewSpecification : Specification<ReviewEntity>
{
    public ReviewSpecification(string? ShopName, int? Rating, ReviewStatusEnum? Status, DateRangeFilterEnum DateRange) : base(Specification<ReviewEntity>.Empty)
    {
        if (!string.IsNullOrEmpty(ShopName))
            AddCriteria(r => r.ShopName.Contains(ShopName));

        if (Rating.HasValue)
            AddCriteria(r => r.Rating == Rating.Value);

        if (Status.HasValue)
            AddCriteria(r => r.Status == Status.Value);

        if (DateRange != DateRangeFilterEnum.All)
        {
            DateTime? dateFilter = DateRange switch
            {
                DateRangeFilterEnum.Today => DateTime.UtcNow.Date,
                DateRangeFilterEnum.Last7Days => DateTime.UtcNow.AddDays(-7),
                DateRangeFilterEnum.LastMonth => DateTime.UtcNow.AddMonths(-1),
                _ => (DateTime?)null
            };

            if (dateFilter.HasValue)
                AddCriteria(r => r.DateSubmitted >= dateFilter.Value);
        }
    }
}
