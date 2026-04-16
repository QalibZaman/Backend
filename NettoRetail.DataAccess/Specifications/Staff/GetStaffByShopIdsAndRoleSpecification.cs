using NettoRetail.Domain.Entities;
using NettoRetail.Shared.Specifications;

namespace NettoRetail.DataAccess.Specifications.Staff;

public class GetStaffByShopIdsAndRoleSpecification : Specification<StaffEntity>
{
    public GetStaffByShopIdsAndRoleSpecification(IReadOnlyCollection<int> shopIds, int roleId)
        : base(x => x.ShopId.HasValue && shopIds.Contains(x.ShopId.Value) && x.RoleId == roleId) { }
}
