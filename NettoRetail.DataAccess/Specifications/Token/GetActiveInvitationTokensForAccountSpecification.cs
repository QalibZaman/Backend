using NettoRetail.Domain.Entities;
using NettoRetail.Shared.Specifications;

namespace NettoRetail.DataAccess.Specifications.Token;

public class GetActiveInvitationTokensForAccountSpecification : Specification<InvitationTokenEntity>
{
    public GetActiveInvitationTokensForAccountSpecification(Guid accountId)
        : base(t => t.AccountId == accountId &&
                    t.UsedAt == null && t.InvalidatedAt == null) { }
}