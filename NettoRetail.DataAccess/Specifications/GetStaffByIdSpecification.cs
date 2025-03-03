using NettoRetail.Domain.Entities;
using NettoRetail.Shared.Specifications;

namespace NettoRetail.DataAccess.Specifications;
public class GetStaffByIdSpecification : Specification<StaffEntity>
{
    public GetStaffByIdSpecification(string id)
    : base(x => x.Id.ToString() == id) { }
}