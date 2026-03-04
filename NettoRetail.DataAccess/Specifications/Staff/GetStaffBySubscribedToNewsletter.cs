using NettoRetail.Domain.Entities;
using NettoRetail.Shared.Specifications;

namespace NettoRetail.DataAccess.Specifications.Account;

public class GetStaffBySubscribedToNewsletter : Specification<StaffEntity>
{
    public GetStaffBySubscribedToNewsletter(bool isSubscribed)
    : base(x => x.IsSubscribedToNewsletter == isSubscribed) { }
}
