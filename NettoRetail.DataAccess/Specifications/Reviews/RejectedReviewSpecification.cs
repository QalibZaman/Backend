using NettoRetail.Domain.Entities;
using NettoRetail.Domain.Enums;
using NettoRetail.Shared.Specifications;

namespace NettoRetail.DataAccess.Specifications.Reviews;

public class RejectedReviewSpecification : Specification<ReviewEntity>
{
    public RejectedReviewSpecification(Guid reviewId) : base(r => r.Id == reviewId && (r.Status == ReviewStatusEnum.Pending || r.Status == ReviewStatusEnum.Approved)) 
    {
    }
}
