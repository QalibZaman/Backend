using NettoRetail.Domain.Entities;
using NettoRetail.Shared.Specifications;

namespace NettoRetail.DataAccess.Specifications.Account;
public class GetAccountByIdSpecification : Specification<AccountEntity>
{
    public GetAccountByIdSpecification(Guid id)
    : base(x => x.Id == id) { }
}