using Microsoft.EntityFrameworkCore;
using NettoRetail.Domain.Entities;
using NettoRetail.Domain.Enums;
using NettoRetail.Shared.Specifications;

namespace NettoRetail.DataAccess.Specifications.Feedback;

public class FeedbackSpecification : Specification<FeedbackEntity>
{
    public FeedbackSpecification(
        string? shopName,
        IReadOnlyCollection<int>? ratings,
        IReadOnlyCollection<FeedbackStatusEnum>? statuses,
        IReadOnlyCollection<bool>? visibilityTypes,
        DateRangeFilterEnum dateRange)
        : base(Specification<FeedbackEntity>.Empty)
    {
        if (!string.IsNullOrWhiteSpace(shopName))
        {
            var normalizedName = shopName.Trim();
            AddCriteria(f => EF.Functions.ILike(f.ShopName, $"%{normalizedName}%"));
        }

        if (ratings is { Count: > 0 })
        {
            AddCriteria(f => ratings.Contains(f.Rating));
        }

        if (statuses is { Count: > 0 })
        {
            AddCriteria(f => statuses.Contains(f.Status));
        }

        if (visibilityTypes is { Count: > 0 })
        {
            AddCriteria(f => visibilityTypes.Contains(f.IsPublic));
        }

        if (dateRange != DateRangeFilterEnum.All)
        {
            var todayUtc = DateTime.UtcNow.Date;
            var startDate = dateRange switch
            {
                DateRangeFilterEnum.Today => todayUtc,
                DateRangeFilterEnum.Last7Days => todayUtc.AddDays(-6),
                DateRangeFilterEnum.LastMonth => todayUtc.AddDays(-29),
                _ => (DateTime?)null
            };

            if (startDate.HasValue)
            {
                var endExclusive = todayUtc.AddDays(1);
                AddCriteria(f => f.DateSubmitted >= startDate.Value && f.DateSubmitted < endExclusive);
            }
        }
    }
}
