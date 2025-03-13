using NettoRetail.Domain.Entities;
using NettoRetail.Domain.Enums;
using NettoRetail.Shared.Specifications;

namespace NettoRetail.DataAccess.Specifications.Role;

public class GetRoleByNameSpecification : Specification<RoleEntity>
{
    public GetRoleByNameSpecification(RoleEnum roleName)
        : base(role => role.RoleName == roleName) { }
}