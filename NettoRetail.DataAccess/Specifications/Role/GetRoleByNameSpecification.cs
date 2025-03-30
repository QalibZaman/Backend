using NettoRetail.Domain.Entities;
using NettoRetail.Shared.Specifications;

namespace NettoRetail.DataAccess.Specifications.Role;

public class GetRoleByNameSpecification : Specification<RoleEntity>
{
    public GetRoleByNameSpecification(string name)
        : base(role => role.Name.ToLower() == name.ToLower()) { }
}