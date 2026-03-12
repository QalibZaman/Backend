using NettoRetail.Application.Models.Feedback;
using NettoRetail.Application.Models.Request;
using NettoRetail.Application.Utilities;
using NettoRetail.DataAccess.Specifications.Feedback;
using NettoRetail.Domain.Entities;
using NettoRetail.Domain.Enums;
using NettoRetail.Shared.Specifications;

namespace NettoRetail.Application.Services.IFeedbackService;

public class FilteredFeedbackService : IFilteredFeedbackService
{
    public Specification<FeedbackEntity> BuildFilteredFeedbacksSpecification(GetFilteredFeedbacksModel model, PaginationRequest pagination)
    {
        var shopName = NormalizeShopName(model.ShopName);
        var ratings = NormalizeRatings(model.Ratings);
        var statuses = NormalizeStatuses(model.Statuses);
        var visibilityTypes = NormalizeVisibilityTypes(model.IsPublic);

        var specification = new FeedbackSpecification(
            shopName,
            ratings,
            statuses,
            visibilityTypes,
            model.DateRange);

        specification.AddInclude(f => f.Account);
        specification.AddInclude(f => f.Account.Shop!);
        specification.AddOrderBy(f => f.DateSubmitted, descending: true);

        var skip = PaginationHelper.CalculateSkip(pagination.PageNumber, pagination.PageSize);
        specification.Skip = skip;
        specification.Take = pagination.PageSize;

        return specification;
    }

    private static string? NormalizeShopName(string? shopName)
        => string.IsNullOrWhiteSpace(shopName) ? null : shopName.Trim();

    private static IReadOnlyCollection<int>? NormalizeRatings(IEnumerable<int>? ratings)
    {
        var normalized = ratings?
            .Where(r => r is >= 1 and <= 5)
            .Distinct()
            .ToList();

        return normalized?.Count > 0 ? normalized : null;
    }

    private static IReadOnlyCollection<FeedbackStatusEnum>? NormalizeStatuses(IEnumerable<FeedbackStatusEnum>? statuses)
    {
        var normalized = statuses?
            .Where(s => Enum.IsDefined(typeof(FeedbackStatusEnum), s))
            .Distinct()
            .ToList();

        return normalized?.Count > 0 ? normalized : null;
    }

    private static IReadOnlyCollection<bool>? NormalizeVisibilityTypes(IEnumerable<bool>? visibilityTypes)
    {
        var normalized = visibilityTypes?
            .Distinct()
            .ToList();

        return normalized?.Count > 0 ? normalized : null;
    }
}
