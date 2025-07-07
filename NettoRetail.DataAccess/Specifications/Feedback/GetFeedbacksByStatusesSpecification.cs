using NettoRetail.Domain.Entities;
using NettoRetail.Domain.Enums;
using NettoRetail.Shared.Specifications;

namespace NettoRetail.DataAccess.Specifications.Feedback;

public class GetFeedbacksByStatusesSpecification : Specification<FeedbackEntity>
{
  public GetFeedbacksByStatusesSpecification(List<FeedbackStatusEnum> statuses)
      : base(x => statuses.Contains(x.Status))
{
}
}