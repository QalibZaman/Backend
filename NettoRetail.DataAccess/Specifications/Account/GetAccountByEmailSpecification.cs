using NettoRetail.Domain.Entities;
using NettoRetail.Shared.Specifications;

namespace NettoRetail.DataAccess.Specifications.Account;
public class GetAccountByEmailSpecification : Specification<AccountEntity>
{
    public GetAccountByEmailSpecification(string email)
    : base(x => x.Email == email) { }
}