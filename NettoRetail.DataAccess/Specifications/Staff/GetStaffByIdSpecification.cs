using NettoRetail.Domain.Entities;
using NettoRetail.Shared.Specifications;

namespace NettoRetail.DataAccess.Specifications.Account;
public class GetStaffByIdSpecification : Specification<StaffEntity>
{
    public GetStaffByIdSpecification(Guid id)
    : base(x => x.Id == id) { }
}