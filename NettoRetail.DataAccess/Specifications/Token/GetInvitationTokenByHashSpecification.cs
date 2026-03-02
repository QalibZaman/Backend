using NettoRetail.Domain.Entities;
using NettoRetail.Shared.Specifications;

namespace NettoRetail.DataAccess.Specifications.Token;

public class GetInvitationTokenByHashSpecification : Specification<InvitationTokenEntity>
{
    public GetInvitationTokenByHashSpecification(string tokenHash)
        : base(t => t.TokenHash == tokenHash)
    {
        AddInclude(t => t.StaffEntity);
    }
}