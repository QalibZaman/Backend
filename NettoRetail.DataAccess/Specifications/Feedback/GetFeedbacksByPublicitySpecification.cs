using NettoRetail.Domain.Entities;
using NettoRetail.Shared.Specifications;

namespace NettoRetail.DataAccess.Specifications.Feedback;

public class GetFeedbacksByPublicitySpecification : Specification<FeedbackEntity>
{
  public GetFeedbacksByPublicitySpecification(List<bool> isPublicList)
      : base(x => isPublicList.Contains(x.IsPublic))
{
}
}