using System.Security.Cryptography;

namespace NettoHealth.Shared.Utilities;

public static class PasswordHasher
{
    private const int SaltSize = 16;
    private const int HashSize = 32;
    private const int Iterations = 100000;
    private static readonly HashAlgorithmName Algorithm = HashAlgorithmName.SHA256;

    public static string HashPassword(string password)
    {
        var salt = RandomNumberGenerator.GetBytes(SaltSize);
        var hash = Rfc2898DeriveBytes.Pbkdf2(password, salt, Iterations, Algorithm, HashSize);
        var hashToString = Convert.ToHexString(hash);
        var saltToString = Convert.ToHexString(salt);
        var passwordHash = $"{hashToString}-{saltToString}";

        return passwordHash;
    }

    public static bool VerifyPassword(string password, string passwordHash)
    {
        var parts = passwordHash.Split("-");
        var hash = Convert.FromHexString(parts[0]);
        var salt = Convert.FromHexString(parts[1]);
        var inputHash = Rfc2898DeriveBytes.Pbkdf2(password, salt, Iterations, Algorithm, HashSize);

        return CryptographicOperations.FixedTimeEquals(hash, inputHash);
    }
}