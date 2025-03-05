using NettoRetail.Domain.Entities;
using NettoRetail.Shared.Specifications;

namespace NettoRetail.DataAccess.Specifications;
public class GetSessionsByRefreshTokenSpecification : Specification<SessionEntity>
{
    public GetSessionsByRefreshTokenSpecification(string refreshToken)
    : base(x => x.Refresh == refreshToken) { }
}