using NettoRetail.Domain.Entities;
using NettoRetail.Shared.Specifications;

namespace NettoRetail.DataAccess.Specifications.Feedback;

public class GetFeedbacksByDateRangeSpecification : Specification<FeedbackEntity>
{
    public GetFeedbacksByDateRangeSpecification(DateTime from, DateTime to)
        : base(f => f.DateSubmitted >= from && f.DateSubmitted <= to)
    {
    }
}
