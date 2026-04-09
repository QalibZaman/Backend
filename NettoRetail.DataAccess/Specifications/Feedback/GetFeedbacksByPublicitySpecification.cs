using NettoRetail.Domain.Entities;
using NettoRetail.Shared.Specifications;

namespace NettoRetail.DataAccess.Specifications.Feedback;

public class GetFeedbacksByPublicitySpecification : Specification<FeedbackEntity>
{
    public GetFeedbacksByPublicitySpecification(IReadOnlyCollection<bool> visibilityTypes)
        : base(f => visibilityTypes.Contains(f.IsPublic))
    {
    }
}
