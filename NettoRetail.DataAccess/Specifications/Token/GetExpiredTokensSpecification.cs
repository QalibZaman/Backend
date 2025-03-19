using NettoRetail.Domain.Entities;
using NettoRetail.Shared.Specifications;
using NettoRetail.Shared.Utilities;

namespace NettoRetail.DataAccess.Specifications.Token
{
    public class GetExpiredTokensSpecification : Specification<BlacklistedTokenEntity>
    {
        public GetExpiredTokensSpecification()
            : base(s => s.ExpiresAt <= DateTime.UtcNow) { }
    }
}
