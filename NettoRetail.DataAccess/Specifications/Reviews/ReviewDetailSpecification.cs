using NettoRetail.Domain.Entities;
using NettoRetail.Shared.Specifications;

namespace NettoRetail.DataAccess.Specifications.Reviews;

public class ReviewDetailSpecification : Specification<ReviewEntity>
{
    public ReviewDetailSpecification(Guid reviewId) : base(r => r.Id == reviewId)
    {
    }
}
