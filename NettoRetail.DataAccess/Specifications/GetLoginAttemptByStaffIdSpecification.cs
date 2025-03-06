using NettoRetail.Domain.Entities;
using NettoRetail.Shared.Specifications;

namespace NettoRetail.DataAccess.Specifications;
public class GetLoginAttemptByStaffIdSpecification : Specification<LoginAttemptEntity>
{
    public GetLoginAttemptByStaffIdSpecification(Guid id)
    : base(x => x.StaffId == id) { }
}
