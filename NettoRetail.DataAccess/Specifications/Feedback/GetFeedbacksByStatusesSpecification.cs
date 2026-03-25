using NettoRetail.Domain.Entities;
using NettoRetail.Domain.Enums;
using NettoRetail.Shared.Specifications;

namespace NettoRetail.DataAccess.Specifications.Feedback;

public class GetFeedbacksByStatusesSpecification : Specification<FeedbackEntity>
{
    public GetFeedbacksByStatusesSpecification(IReadOnlyCollection<FeedbackStatusEnum> statuses)
        : base(f => statuses.Contains(f.Status))
    {
    }
}
