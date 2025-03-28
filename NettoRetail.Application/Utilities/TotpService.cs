using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using NettoRetail.Application.Interfaces;
using NettoRetail.Shared.Options;
using NettoRetail.Shared.Utilities;
using OtpNet;

namespace NettoRetail.Application.Utilities;

public class TotpService(IOptions<TotpOptions> options, IOptions<TotpEncryptionOptions> encryptionOptions, ILogger<TotpService> logger) : ITotpService
{
    private const int SecretKeyLength = 20;
    private readonly TotpOptions _options = options.Value;
    private readonly string _encryptionKey = encryptionOptions.Value.EncryptionKeyBase64;
    private readonly ILogger<TotpService> _logger = logger;

    public string GenerateSecret()
    {
        var secret = KeyGeneration.GenerateRandomKey(SecretKeyLength);
        return Base32Encoding.ToString(secret);
    }

    public string GenerateQrCodeUri(string userEmail, string secret)
    {
        if (string.IsNullOrEmpty(userEmail) || string.IsNullOrEmpty(secret))
        {
            _logger.LogWarning("Attempt to generate QR code URI with empty values. UserEmail: {UserEmail}, Secret: {Secret}", userEmail, secret);
            throw new ArgumentNullException("User email and secret are required.");
        }

        return $"otpauth://totp/{Uri.EscapeDataString(_options.Issuer)}:{Uri.EscapeDataString(userEmail)}" +
               $"?secret={secret}&issuer={Uri.EscapeDataString(_options.Issuer)}" +
               $"&digits={_options.Digits}&period={_options.PeriodInSeconds}";
    }

    public string GenerateCode(string encryptedSecret)
    {
        if (string.IsNullOrEmpty(encryptedSecret))
            throw new ArgumentNullException(nameof(encryptedSecret));
        try
        {
            var secretKey = EncryptionUtility.Decrypt(encryptedSecret, _encryptionKey);
            if (secretKey == null)
            {
                throw new ArgumentException("Decryption failed.");
            }

            var keyBytes = Base32Encoding.ToBytes(secretKey);
            var totp = new Totp(
                keyBytes,
                step: _options.PeriodInSeconds,
                mode: _options.HashMode,
                totpSize: _options.Digits);

            return totp.ComputeTotp();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "TOTP generation failed.");
            throw new InvalidOperationException("TOTP generation failed due to an internal error.");
        }
    }

    public string EncryptSecret(string plainSecret)
    {
        try
        {
            return EncryptionUtility.Encrypt(plainSecret, _encryptionKey);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "TOTP secret encryption failed.");
            throw new InvalidOperationException("An error occurred while encrypting the secret.");
        }
    }

    public bool VerifyCode(string encryptedSecret, string codeFromUser)
    {
        if (string.IsNullOrEmpty(encryptedSecret) || string.IsNullOrEmpty(codeFromUser))
            throw new ArgumentNullException("Secret and code are required.");
        try
        {
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
        catch (Exception ex)
        {
            _logger.LogError(ex, "Unexpected error during TOTP verification.");
            throw new InvalidOperationException("TOTP verification failed.");
        }
    }
}
