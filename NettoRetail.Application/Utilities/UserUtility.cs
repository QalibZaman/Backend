using System.Text;
using Bogus;

namespace NettoRetail.Application.Utilities;

public static class UserUtility
{
    public static string GenerateLoyaltyId()
    {
        var faker = new Faker();
        // Result will be in the format of ABC-DEF-123.
        var result = $"{faker.Random.String2(3).ToUpper()}-{faker.Random.String2(3).ToUpper()}-{faker.Random.Number(0, 999):D3}";
        return result;
    }
}