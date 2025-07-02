using NettoRetail.Domain.Entities;
using NettoRetail.Shared.Specifications;

namespace NettoRetail.DataAccess.Specifications.Feedback;

  public class GetFeedbacksByPublicitySpecification(List<bool> isPublicList) : Specification<FeedbackEntity>(x => isPublicList.Contains(x.IsPublic))
{
}
