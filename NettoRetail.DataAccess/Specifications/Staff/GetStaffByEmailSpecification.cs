using NettoRetail.Domain.Entities;
using NettoRetail.Shared.Specifications;

namespace NettoRetail.DataAccess.Specifications.Account;
public class GetStaffByEmailSpecification : Specification<StaffEntity>
{
    public GetStaffByEmailSpecification(string email)
    : base(x => x.Email == email) { }
}