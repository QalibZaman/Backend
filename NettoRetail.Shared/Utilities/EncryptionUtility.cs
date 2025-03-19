using System.Security.Cryptography;
using System.Text;

namespace NettoRetail.Shared.Utilities;

public static class EncryptionUtility
{
    private const int AesBlockSize = 16;
    private const int AesKeySize = 32;

    public static string Encrypt(string plainText, string base64Key)
    {
        if (string.IsNullOrEmpty(plainText))
            throw new ArgumentException("Encryption failed: input text is empty.");

        if (!IsValidKey(base64Key))
            throw new ArgumentException("Encryption failed: invalid encryption key.");

        var keyBytes = Convert.FromBase64String(base64Key);
        var ivBytes = GenerateInitializationVector();

        using var aes = CreateAesCipher(keyBytes, ivBytes);
        using var encryptor = aes.CreateEncryptor();
        var encryptedBytes = PerformCryptoTransform(encryptor, Encoding.UTF8.GetBytes(plainText));

        return Convert.ToBase64String(CombineIvAndEncryptedData(ivBytes, encryptedBytes));
    }

    public static string Decrypt(string encryptedText, string base64Key)
    {
        if (string.IsNullOrEmpty(encryptedText))
            throw new ArgumentException("Decryption failed: input text is empty.");

        if (!IsValidKey(base64Key))
            throw new ArgumentException("Decryption failed: invalid decryption key.");

        var keyBytes = Convert.FromBase64String(base64Key);
        var combinedBytes = Convert.FromBase64String(encryptedText);

        if (combinedBytes.Length < AesBlockSize)
            throw new ArgumentException("Decryption failed: input data is too short.");

        var (ivBytes, encryptedBytes) = SplitIvAndEncryptedData(combinedBytes);

        using var aes = CreateAesCipher(keyBytes, ivBytes);
        using var decryptor = aes.CreateDecryptor();

        return Encoding.UTF8.GetString(PerformCryptoTransform(decryptor, encryptedBytes));
    }

    private static Aes CreateAesCipher(byte[] key, byte[] iv)
    {
        var aes = Aes.Create();
        aes.Key = key;
        aes.IV = iv;
        aes.Mode = CipherMode.CBC;
        aes.Padding = PaddingMode.PKCS7;
        return aes;
    }

    private static byte[] PerformCryptoTransform(ICryptoTransform transform, byte[] data)
    {
        return transform.TransformFinalBlock(data, 0, data.Length);
    }

    private static byte[] GenerateInitializationVector()
    {
        var iv = new byte[AesBlockSize];
        RandomNumberGenerator.Fill(iv);
        return iv;
    }

    private static byte[] CombineIvAndEncryptedData(byte[] iv, byte[] encryptedData)
    {
        var combined = new byte[AesBlockSize + encryptedData.Length];
        Buffer.BlockCopy(iv, 0, combined, 0, AesBlockSize);
        Buffer.BlockCopy(encryptedData, 0, combined, AesBlockSize, encryptedData.Length);
        return combined;
    }

    private static (byte[] iv, byte[] encryptedData) SplitIvAndEncryptedData(byte[] combinedData)
    {
        var iv = new byte[AesBlockSize];
        var encryptedData = new byte[combinedData.Length - AesBlockSize];

        Buffer.BlockCopy(combinedData, 0, iv, 0, AesBlockSize);
        Buffer.BlockCopy(combinedData, AesBlockSize, encryptedData, 0, encryptedData.Length);

        return (iv, encryptedData);
    }

    private static bool IsValidKey(string base64Key)
    {
        if (string.IsNullOrEmpty(base64Key))
            return false;

        try
        {
            var keyBytes = Convert.FromBase64String(base64Key);
            return keyBytes.Length == AesKeySize;
        }
        catch (FormatException)
        {
            return false;
        }
    }
}
