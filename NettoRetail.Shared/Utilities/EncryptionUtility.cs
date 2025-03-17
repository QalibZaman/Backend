using System.Security.Cryptography;
using System.Text;

namespace NettoRetail.Shared.Utilities;

public static class EncryptionUtility
{
    private const int AesBlockSize = 16; 
    private const int AesKeySize = 32;   

    public static string Encrypt(string plainText, string base64Key)
    {
        if (string.IsNullOrEmpty(plainText) || string.IsNullOrEmpty(base64Key))
            return string.Empty;

        var keyBytes = Convert.FromBase64String(base64Key);
        if (keyBytes.Length != AesKeySize)
            throw new ArgumentException($"Key must be {AesKeySize} bytes long.");

        var ivBytes = GenerateInitializationVector();

        using var aes = Aes.Create();
        aes.Key = keyBytes;
        aes.IV = ivBytes;
        aes.Mode = CipherMode.CBC;
        aes.Padding = PaddingMode.PKCS7;

        using var encryptor = aes.CreateEncryptor();
        var plainBytes = Encoding.UTF8.GetBytes(plainText);
        var encryptedBytes = encryptor.TransformFinalBlock(plainBytes, 0, plainBytes.Length);

        var combinedBytes = new byte[AesBlockSize + encryptedBytes.Length];
        Buffer.BlockCopy(ivBytes, 0, combinedBytes, 0, AesBlockSize);
        Buffer.BlockCopy(encryptedBytes, 0, combinedBytes, AesBlockSize, encryptedBytes.Length);

        return Convert.ToBase64String(combinedBytes);
    }

    public static string Decrypt(string encryptedText, string base64Key)
    {
        if (string.IsNullOrEmpty(encryptedText) || string.IsNullOrEmpty(base64Key))
            return string.Empty;

        var keyBytes = Convert.FromBase64String(base64Key);
        if (keyBytes.Length != AesKeySize)
            throw new ArgumentException($"Key must be {AesKeySize} bytes long.");

        var combinedBytes = Convert.FromBase64String(encryptedText);

        if (combinedBytes.Length < AesBlockSize)
            return string.Empty; 

        var ivBytes = new byte[AesBlockSize];
        var encryptedBytes = new byte[combinedBytes.Length - AesBlockSize];

        Buffer.BlockCopy(combinedBytes, 0, ivBytes, 0, AesBlockSize);
        Buffer.BlockCopy(combinedBytes, AesBlockSize, encryptedBytes, 0, encryptedBytes.Length);

        using var aes = Aes.Create();
        aes.Key = keyBytes;
        aes.IV = ivBytes;
        aes.Mode = CipherMode.CBC;
        aes.Padding = PaddingMode.PKCS7;

        try
        {
            using var decryptor = aes.CreateDecryptor();
            var decryptedBytes = decryptor.TransformFinalBlock(encryptedBytes, 0, encryptedBytes.Length);
            return Encoding.UTF8.GetString(decryptedBytes);
        }
        catch (CryptographicException ex)
        {
            Console.WriteLine($"Decryption failed: {ex.Message}");
            return string.Empty; 
        }
    }

    private static byte[] GenerateInitializationVector()
    {
        var iv = new byte[AesBlockSize];
        RandomNumberGenerator.Fill(iv);
        return iv;
    }
}
