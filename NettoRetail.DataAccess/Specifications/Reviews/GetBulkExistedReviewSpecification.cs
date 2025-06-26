using NettoRetail.Domain.Entities;
using NettoRetail.Shared.Specifications;

namespace NettoRetail.DataAccess.Specifications.Reviews;

public class GetBulkExistedReviewSpecification : Specification<ReviewEntity>
{
    public GetBulkExistedReviewSpecification(List<Guid> reviewIds) : base(r => reviewIds.Contains(r.Id))
    {
    }
}