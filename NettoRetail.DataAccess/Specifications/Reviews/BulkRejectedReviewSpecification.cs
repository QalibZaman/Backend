using NettoRetail.Domain.Entities;
using NettoRetail.Domain.Enums;
using NettoRetail.Shared.Specifications;

namespace NettoRetail.DataAccess.Specifications.Reviews;

public class BulkRejectedReviewSpecification : Specification<ReviewEntity>
{
    public BulkRejectedReviewSpecification(List<Guid> reviewIds) : base(r => reviewIds.Contains(r.Id) && (r.Status == ReviewStatusEnum.Pending || r.Status == ReviewStatusEnum.Approved))
    {
    }
}
