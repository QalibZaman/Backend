using NettoRetail.Domain.Entities;
using NettoRetail.Shared.Specifications;

namespace NettoRetail.DataAccess.Specifications.Staff;

public class GetStaffBySubscribedToNewsletter : Specification<StaffEntity>
{
    public GetStaffBySubscribedToNewsletter(bool isSubscribed)
    : base(x => x.IsSubscribedToNewsletter == isSubscribed) { }
}
