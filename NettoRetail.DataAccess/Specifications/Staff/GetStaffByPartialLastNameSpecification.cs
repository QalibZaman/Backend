using Microsoft.EntityFrameworkCore;
using NettoRetail.Domain.Entities;
using NettoRetail.Shared.Specifications;

namespace NettoRetail.DataAccess.Specifications.Staff;

public class GetStaffByPartialLastNameSpecification : Specification<StaffEntity>
{
    public GetStaffByPartialLastNameSpecification(string partialName)
        : base(x => EF.Functions.ILike(x.LastName == null ? "" : x.LastName, $"%{partialName}%")) { }
}
