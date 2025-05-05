using NettoRetail.Domain.Entities;
using NettoRetail.Shared.Specifications;

namespace NettoRetail.DataAccess.Specifications.Sessions;
public class GetSessionsByRefreshTokenSpecification : Specification<SessionEntity>
{
    public GetSessionsByRefreshTokenSpecification(string refreshToken)
    : base(x => x.RefreshToken == refreshToken && x.IsActive) { }
}