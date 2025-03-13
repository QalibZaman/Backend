using NettoRetail.Domain.Entities;
using NettoRetail.Shared.Specifications;

namespace NettoRetail.DataAccess.Specifications.Sessions;
public class GetSessionsByJwtSpecification : Specification<SessionEntity>
{
    public GetSessionsByJwtSpecification(string jwt)
    : base(x => x.Jwt == jwt) { }
}