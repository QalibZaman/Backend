using NettoRetail.Domain.Entities;
using NettoRetail.Shared.Specifications;

namespace NettoRetail.DataAccess.Specifications.Feedback;

public class GetFeedbacksByAccountIdsSpecification(List<Guid> accountIds)
    : Specification<FeedbackEntity>(x => x.AccountId.HasValue && accountIds.Contains(x.AccountId.Value))
{
}