using NettoRetail.Domain.Entities;
using NettoRetail.Shared.Specifications;

namespace NettoRetail.DataAccess.Specifications.Feedback;
public class GetFeedbacksByDateRangeSpecification(DateTime from, DateTime to) : Specification<FeedbackEntity>(x => x.DateSubmitted >= from && x.DateSubmitted <= to)
{
}