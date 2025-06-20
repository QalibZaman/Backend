using NettoRetail.Domain.Entities;
using NettoRetail.Domain.Enums;
using NettoRetail.Shared.Specifications;

namespace NettoRetail.DataAccess.Specifications.Feedback;

public class GetPublicFeedbacksSpecification : Specification<FeedbackEntity>
{
  public GetPublicFeedbacksSpecification()
      : base(f => f.IsPublic && f.Status == FeedbackStatusEnum.Approved)
  {
    AddOrderBy(f => f.DateSubmitted, descending: true);
  }
}
