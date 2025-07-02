using NettoRetail.Domain.Entities;
using NettoRetail.Shared.Specifications;

namespace NettoRetail.DataAccess.Specifications.Feedback;

  public class GetFeedbacksByRatingsSpecification(List<int> ratings) : Specification<FeedbackEntity>(x => ratings.Contains(x.Rating))
{
}
