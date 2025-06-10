using NettoRetail.Domain.Entities;
using NettoRetail.Domain.Enums;
using NettoRetail.Shared.Specifications;

namespace NettoRetail.DataAccess.Specifications.Reviews;

public class BulkApproveReviewsSpecification : Specification<ReviewEntity>
{
    public BulkApproveReviewsSpecification(List<Guid> reviewIds) : base(r => reviewIds.Contains(r.Id) && (r.Status == ReviewStatusEnum.Pending || r.Status == ReviewStatusEnum.Rejected))
    {
    }
}
