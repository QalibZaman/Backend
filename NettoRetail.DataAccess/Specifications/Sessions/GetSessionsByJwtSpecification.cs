using NettoRetail.Domain.Entities;
using NettoRetail.Shared.Specifications;

namespace NettoRetail.DataAccess.Specifications.Sessions;

// This specification finds an active session in the database
// by its access token (jwt). We use it to check if the user
// still has a valid session before we log them out.
public class GetSessionsByJwtSpecification(string jwt) : Specification<SessionEntity>(x => x.Jwt == jwt && x.IsActive == true)
{
}