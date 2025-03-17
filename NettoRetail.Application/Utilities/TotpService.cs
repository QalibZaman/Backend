using Microsoft.Extensions.Options;
using NettoRetail.Application.Interfaces;
using NettoRetail.Shared.Options;
using NettoRetail.Shared.Utilities;
using OtpNet;

namespace NettoRetail.Application.Utilities;

public class TotpService(IOptions<TotpOptions> options, IOptions<TotpEncryptionOptions> encryptionOptions) : ITotpService
{
    private readonly TotpOptions _options = options.Value;
    private readonly string _encryptionKey = encryptionOptions.Value.EncryptionKeyBase64;

    public string GenerateSecret()
    {
        var secret = KeyGeneration.GenerateRandomKey(20);
        return Base32Encoding.ToString(secret);
    }

    public string GenerateQrCodeUri(string userEmail, string secret)
    {
        if (string.IsNullOrEmpty(userEmail) || string.IsNullOrEmpty(secret))
            throw new ArgumentNullException("User email and secret are required.");

        return $"otpauth://totp/{Uri.EscapeDataString(_options.Issuer)}:{Uri.EscapeDataString(userEmail)}" +
               $"?secret={secret}&issuer={Uri.EscapeDataString(_options.Issuer)}" +
               $"&digits={_options.Digits}&period={_options.PeriodInSeconds}";
    }

    public string GenerateCode(string encryptedSecret)
    {
        if (string.IsNullOrEmpty(encryptedSecret))
            throw new ArgumentNullException(nameof(encryptedSecret));

        var secretKey = EncryptionUtility.Decrypt(encryptedSecret, _encryptionKey);
        if (secretKey == null) throw new InvalidOperationException("Decryption failed.");

        var keyBytes = Base32Encoding.ToBytes(secretKey);
        var totp = new Totp(
            keyBytes,
            step: _options.PeriodInSeconds,
            mode: _options.HashMode,
            totpSize: _options.Digits
        );

        return totp.ComputeTotp();
    }

    public bool VerifyCode(string encryptedSecret, string codeFromUser)
    {
        if (string.IsNullOrEmpty(encryptedSecret) || string.IsNullOrEmpty(codeFromUser))
            throw new ArgumentNullException("Secret and code are required.");

        var secretKey = EncryptionUtility.Decrypt(encryptedSecret, _encryptionKey);
        if (secretKey == null) return false;

        var keyBytes = Base32Encoding.ToBytes(secretKey);
        var totp = new Totp(
            keyBytes,
            step: _options.PeriodInSeconds,
            mode: _options.HashMode,
            totpSize: _options.Digits);

        var verificationWindow = new VerificationWindow(previous: 1, future: 1);
        return totp.VerifyTotp(codeFromUser, out _, verificationWindow);
    }

    public string EncryptSecret(string plainSecret)
    {
        return EncryptionUtility.Encrypt(plainSecret, _encryptionKey);
    }
}
