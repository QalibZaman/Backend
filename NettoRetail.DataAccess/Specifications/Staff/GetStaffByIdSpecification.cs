using NettoRetail.Domain.Entities;
using NettoRetail.Shared.Specifications;

namespace NettoRetail.DataAccess.Specifications.Staff;
public class GetStaffByIdSpecification : Specification<StaffEntity>
{
    public GetStaffByIdSpecification(Guid id)
    : base(x => x.Id == id) { }
}