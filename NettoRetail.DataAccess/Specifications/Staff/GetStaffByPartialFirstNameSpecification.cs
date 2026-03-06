using Microsoft.EntityFrameworkCore;
using NettoRetail.Domain.Entities;
using NettoRetail.Shared.Specifications;

namespace NettoRetail.DataAccess.Specifications.Account;

public class GetStaffByPartialFirstNameSpecification : Specification<StaffEntity>
{
    public GetStaffByPartialFirstNameSpecification(string partialName)
        : base(x => EF.Functions.ILike(x.FirstName == null ? "" : x.FirstName, $"%{partialName}%")) { }
}
