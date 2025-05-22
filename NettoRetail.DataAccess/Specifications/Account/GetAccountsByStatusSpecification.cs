using NettoRetail.Domain.Entities;
using NettoRetail.Domain.Enums;
using NettoRetail.Shared.Specifications;

namespace NettoRetail.DataAccess.Specifications.Account;

public class GetAccountsByStatusSpecification : Specification<AccountEntity>
{
    public GetAccountsByStatusSpecification(StaffStatusEnum status) : base(x => x.Status == status)
    { }
}
