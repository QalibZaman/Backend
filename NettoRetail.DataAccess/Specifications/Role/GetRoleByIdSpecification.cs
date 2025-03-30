using NettoRetail.Domain.Entities;
using NettoRetail.Shared.Specifications;

namespace NettoRetail.DataAccess.Specifications.Role;
public class GetRoleByIdSpecification : Specification<RoleEntity>
{
    public GetRoleByIdSpecification(int roleId)
        : base(role => role.Id == roleId) { }
}
