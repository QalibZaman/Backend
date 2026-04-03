using NettoRetail.Domain.Entities;
using NettoRetail.Shared.Specifications;

namespace NettoRetail.DataAccess.Specifications.Staff;
public class GetStaffByShopIdSpecification : Specification<StaffEntity>
{
    public GetStaffByShopIdSpecification(int shopId)
        : base(model => model.ShopId == shopId) { }
}
