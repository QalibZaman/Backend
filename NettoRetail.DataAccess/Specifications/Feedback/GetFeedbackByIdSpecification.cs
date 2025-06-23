using NettoRetail.Domain.Entities;
using NettoRetail.Shared.Specifications;

namespace NettoRetail.DataAccess.Specifications.Feedback;

public class GetFeedbackByIdSpecification(Guid id) : Specification<FeedbackEntity>(o => o.Id == id)
{
}