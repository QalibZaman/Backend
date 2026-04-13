using NettoRetail.Domain.Entities;
using NettoRetail.Domain.Enums;
using NettoRetail.Shared.Specifications;

namespace NettoRetail.DataAccess.Specifications.Staff;

public class GetStaffByStatusSpecification : Specification<StaffEntity>
{
    public GetStaffByStatusSpecification(StaffStatusEnum status) : base(x => x.Status == status)
    { }
}
