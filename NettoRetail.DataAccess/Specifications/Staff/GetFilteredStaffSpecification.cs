using Microsoft.EntityFrameworkCore;
using NettoRetail.Domain.Entities;
using NettoRetail.Domain.Enums;
using NettoRetail.Shared.Specifications;

namespace NettoRetail.DataAccess.Specifications.Account;

public class GetFilteredStaffSpecification : Specification<StaffEntity>
{
    public GetFilteredStaffSpecification(
        int shopId,
        string? firstName,
        string? lastName,
        string? email,
        int? roleId,
        StaffStatusEnum? status)
        : base(x =>
            x.ShopId == shopId &&

            (string.IsNullOrEmpty(firstName) || (x.FirstName != null && EF.Functions.ILike(x.FirstName, $"%{firstName}%"))) &&

            (string.IsNullOrEmpty(lastName) || (x.LastName != null && EF.Functions.ILike(x.LastName, $"%{lastName}%"))) &&

            (string.IsNullOrEmpty(email) || (x.Email != null && EF.Functions.ILike(x.Email, $"%{email}%"))) &&

            (!roleId.HasValue || x.RoleId == roleId.Value) &&

            (!status.HasValue || x.Status == status.Value))
    {
        AddInclude(x => x.Role);
    }
}