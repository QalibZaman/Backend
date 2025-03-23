using System.Text;

namespace NettoRetail.Application.Utilities;

public static class UserUtility
{
    private static readonly string[] Vowels = { "A", "E", "I", "O", "U" };
    private static readonly string[] Consonants = { "B", "C", "D", "F", "G", "H", "J", "K", "L", "M", "N", "P", "Q", "R", "S", "T", "V", "W", "X", "Y", "Z" };

    public static string GenerateLoyaltyId()
    {
        return $"{GenerateCVC()}-{GenerateCVC()}-{GenerateRandomNumber()}";
    }

    private static string GenerateCVC()
    {
        var stringBuilder = new StringBuilder();

        stringBuilder.Append(GetRandomConsonant());
        stringBuilder.Append(GetRandomVowel());
        stringBuilder.Append(GetRandomConsonant());

        return stringBuilder.ToString();
    }

    private static string GetRandomConsonant()
    {
        var random = new Random();
        return Consonants[random.Next(Consonants.Length)];
    }

    private static string GetRandomVowel()
    {
        var random = new Random();
        return Vowels[random.Next(Vowels.Length)];
    }

    private static string GenerateRandomNumber()
    {
        var random = new Random();
        return random.Next(0, 999).ToString("D3");
    }
}