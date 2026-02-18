using System.Text;
using Bogus;
using NettoRetail.Shared.Constants;

namespace NettoRetail.Application.Utilities;

public static class UserUtility
{
    public static string GenerateLoyaltyId()
    {
        var faker = new Faker();
        // Result will be in the format of AbC2d.
        var result = faker.Random.String2(5, CharsForRandoming.LoyaltyIdChars);
        return result;
    }
}
