using NettoRetail.Domain.Entities;
using NettoRetail.Shared.Specifications;

namespace NettoRetail.DataAccess.Specifications.Role;

public class GetRoleByNameExcludingCurrentSpecification : Specification<RoleEntity>
{
    public GetRoleByNameExcludingCurrentSpecification(string name, int currentRoleId, int shopId)
        : base(r => r.Name.ToLower() == name.ToLower()
                    && r.ShopId == shopId
                    && currentRoleId != r.Id) { }
}
