using Microsoft.EntityFrameworkCore;
using NettoRetail.Domain.Entities;
using NettoRetail.Shared.Specifications;

namespace NettoRetail.DataAccess.Specifications.Account;

public class GetAccountsByPartialEmailSpecification : Specification<AccountEntity>
{
    public GetAccountsByPartialEmailSpecification(string email)
        : base(x => EF.Functions.ILike(x.Email == null ? "" : x.Email, $"%{email}%")) { }
}
