using NettoRetail.Domain.Entities;
using NettoRetail.Shared.Specifications;

namespace NettoRetail.DataAccess.Specifications.Feedback;

public class GetFeedbacksByRatingsSpecification : Specification<FeedbackEntity>
{
  public GetFeedbacksByRatingsSpecification(List<int> ratings)
      : base(x => ratings.Contains(x.Rating))
  {
  }
}
