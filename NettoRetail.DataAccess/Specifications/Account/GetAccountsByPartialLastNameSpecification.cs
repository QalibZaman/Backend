using Microsoft.EntityFrameworkCore;
using NettoRetail.Domain.Entities;
using NettoRetail.Shared.Specifications;

namespace NettoRetail.DataAccess.Specifications.Account;

public class GetAccountsByPartialLastNameSpecification : Specification<AccountEntity>
{
    public GetAccountsByPartialLastNameSpecification(string partialName)
        : base(x => EF.Functions.ILike(x.LastName == null ? "" : x.LastName, $"%{partialName}%")) { }
}
