using NettoRetail.Domain.Entities;
using NettoRetail.Shared.Specifications;

namespace NettoRetail.DataAccess.Specifications.Feedback;

public class GetFeedbackByIdSpecification : Specification<FeedbackEntity>
{
    public GetFeedbackByIdSpecification(Guid id) : base(o => o.Id == id)
    {
    }
}