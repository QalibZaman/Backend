using NettoRetail.Domain.Entities;
using NettoRetail.Shared.Specifications;

namespace NettoRetail.DataAccess.Specifications.Account;

public class GetAccountsByRoleIdSpecification : Specification<AccountEntity>
{
    public GetAccountsByRoleIdSpecification(int roleId)
    : base(x => x.RoleId == roleId) { }
}
