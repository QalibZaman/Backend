using NettoRetail.Domain.Entities;
using NettoRetail.Shared.Specifications;

namespace NettoRetail.DataAccess.Specifications.Role;

public class GetRoleByIdSpecification : Specification<RoleEntity>
{
    public GetRoleByIdSpecification(int id)
        : base(role => role.Id == id) { }
}