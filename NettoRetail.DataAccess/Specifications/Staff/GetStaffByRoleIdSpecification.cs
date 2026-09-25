using NettoRetail.Domain.Entities;
using NettoRetail.Shared.Specifications;

namespace NettoRetail.DataAccess.Specifications.Staff;

public class GetStaffByRoleIdSpecification : Specification<StaffEntity>
{
    public GetStaffByRoleIdSpecification(int roleId)
    : base(x => x.RoleId == roleId) { }
}
