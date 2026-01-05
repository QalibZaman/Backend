using NettoRetail.Domain.Entities.JunctionEntities;
using NettoRetail.Shared.Specifications;

namespace NettoRetail.DataAccess.Specifications.InvitationToken;

public class GetInvitationTokenByHashSpecification : Specification<InvitationTokenEntity>

{
    public GetInvitationTokenByHashSpecification(string tokenHash)
        : base(t => t.TokenHash == tokenHash) { }
}