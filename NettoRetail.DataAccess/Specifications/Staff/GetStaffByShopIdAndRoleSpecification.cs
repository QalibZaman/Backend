using NettoRetail.Domain.Entities;
using NettoRetail.Shared.Specifications;

namespace NettoRetail.DataAccess.Specifications.Staff;

public class GetStaffByShopIdAndRoleSpecification : Specification<StaffEntity>
{
    public GetStaffByShopIdAndRoleSpecification(int shopId, int roleId)
        : base(x => x.ShopId == shopId && x.RoleId == roleId) { }
}