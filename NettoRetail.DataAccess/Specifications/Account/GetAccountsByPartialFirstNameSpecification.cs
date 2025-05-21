using Microsoft.EntityFrameworkCore;
using NettoRetail.Domain.Entities;
using NettoRetail.Shared.Specifications;

namespace NettoRetail.DataAccess.Specifications.Account;

public class GetAccountsByPartialFirstNameSpecification : Specification<AccountEntity>
{
    public GetAccountsByPartialFirstNameSpecification(string partialName)
        : base(x => EF.Functions.ILike(x.FirstName == null ? "" : x.FirstName, $"%{partialName}%")) { }
}
