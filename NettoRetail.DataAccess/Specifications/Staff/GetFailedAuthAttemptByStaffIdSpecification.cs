using NettoRetail.Domain.Entities;
using NettoRetail.Shared.Specifications;

namespace NettoRetail.DataAccess.Specifications.Staff;
public class GetFailedAuthAttemptByStaffIdSpecification : Specification<FailedAuthAttemptEntity>
{
    public GetFailedAuthAttemptByStaffIdSpecification(Guid id)
    : base(x => x.StaffId == id) { }
}
