using System.IdentityModel.Tokens.Jwt;
using NettoRetail.Domain.Entities;
using NettoRetail.Shared.Specifications;

namespace NettoRetail.DataAccess.Specifications;

public class GetSessionsWithExpiredTokenSpecification : Specification<SessionEntity>
{
    public GetSessionsWithExpiredTokenSpecification()
           : base(x => IsJwtExpired(x.Jwt)) { }

    private static bool IsJwtExpired(string jwtToken)
    {
        if (string.IsNullOrEmpty(jwtToken)) return true;

        var handler = new JwtSecurityTokenHandler();
        try
        {
            var token = handler.ReadJwtToken(jwtToken);
            var expClaim = token?.Claims.FirstOrDefault(c => c.Type == "exp");

            if (expClaim == null) return true;

            var expirationDate = DateTimeOffset.FromUnixTimeSeconds(long.Parse(expClaim.Value)).DateTime;

            return expirationDate > DateTime.UtcNow;
        }
        catch
        {
            return true;
        }
    }
}