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
        DateTime? startDate,
        DateTime? endDate)
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

        if (startDate.HasValue && endDate.HasValue)
        {
            AddCriteria(f => f.DateSubmitted >= startDate.Value && f.DateSubmitted <= endDate.Value);
        }
    }
}