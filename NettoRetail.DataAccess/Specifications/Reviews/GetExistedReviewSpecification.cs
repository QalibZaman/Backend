using NettoRetail.Domain.Entities;
using NettoRetail.Shared.Specifications;

namespace NettoRetail.DataAccess.Specifications.Reviews;
public class GetExistedReviewSpecification : Specification<ReviewEntity>
{
    public GetExistedReviewSpecification(Guid reviewId) : base(r => r.Id == reviewId)
    {
    }
}
