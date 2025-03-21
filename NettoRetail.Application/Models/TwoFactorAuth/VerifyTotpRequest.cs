namespace NettoRetail.Application.Models.TwoFactorAuth;

public class VerifyTotpRequest
{
    public string TotpCode { get; set; } = string.Empty;
}
