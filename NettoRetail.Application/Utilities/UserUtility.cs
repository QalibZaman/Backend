using Bogus;
using NettoRetail.Shared.Constants;

namespace NettoRetail.Application.Utilities;

public static class UserUtility
{
    private const int LoyaltyIdLength = 6;

    public static string GenerateLoyaltyId()
    {
        var faker = new Faker();
        var result = faker.Random.String2(LoyaltyIdLength, CharsForRandoming.LoyaltyIdChars);
        return result;
    }
}
