using NettoRetail.Domain.Entities;
using NettoRetail.Shared.Specifications;

namespace NettoRetail.DataAccess.Specifications.Account;

public class GetStaffByRoleIdSpecification : Specification<StaffEntity>
{
    public GetStaffByRoleIdSpecification(int roleId)
    : base(x => x.RoleId == roleId) { }
}
