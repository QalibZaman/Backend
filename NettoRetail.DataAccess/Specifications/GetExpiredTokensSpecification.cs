using NettoRetail.Domain.Entities;
using NettoRetail.Shared.Specifications;
using NettoRetail.Shared.Utilities;

namespace NettoRetail.DataAccess.Specifications
{
    public class GetExpiredTokensSpecification : Specification<BlacklistedTokenEntity>
    {
        public GetExpiredTokensSpecification()
            : base(t => JwtUtils.IsJwtExpired(t.Token)) { }
    }
}
