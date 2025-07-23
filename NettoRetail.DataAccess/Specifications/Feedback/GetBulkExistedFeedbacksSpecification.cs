using NettoRetail.Domain.Entities;
using NettoRetail.Shared.Specifications;

namespace NettoRetail.DataAccess.Specifications.Feedback;

  public class GetBulkExistedFeedbacksSpecification : Specification<FeedbackEntity>
{
  public GetBulkExistedFeedbacksSpecification(List<Guid> feedbacksIds)
        : base(f => feedbacksIds.Contains(f.Id))
  {
  }
}
