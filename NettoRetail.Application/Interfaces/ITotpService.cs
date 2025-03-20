namespace NettoRetail.Application.Interfaces;
public interface ITotpService
{
    string GenerateSecret();
    string GenerateQrCodeUri(string userEmail, string secretKey);
    string GenerateCode(string secretKey);
    bool VerifyCode(string secretKey, string codeFromUser);
    public string EncryptSecret(string plainSecret);
}
