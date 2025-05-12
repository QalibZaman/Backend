using NettoRetail.Domain.Entities;
using NettoRetail.Shared.Specifications;

namespace NettoRetail.DataAccess.Specifications.Account;

public class GetAccountBySubscribedToNewsletter : Specification<AccountEntity>
{
    public GetAccountBySubscribedToNewsletter(bool isSubscribed)
    : base(x => x.IsSubscribedToNewsletter == isSubscribed) { }
}
