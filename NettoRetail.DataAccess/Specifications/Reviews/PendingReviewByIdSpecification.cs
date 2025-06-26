using NettoRetail.Domain.Entities;
using NettoRetail.Domain.Enums;
using NettoRetail.Shared.Specifications;

namespace NettoRetail.DataAccess.Specifications.Reviews;

public class PendingReviewSpecification : Specification<ReviewEntity>
{
    public PendingReviewSpecification(Guid reviewId) : base(r => r.Id == reviewId && r.Status == ReviewStatusEnum.Pending)
    {
    }
}
