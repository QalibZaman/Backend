namespace NettoRetail.Application.Models.TwoFactorAuth;

public record EnableTwoFactorAuthResponse(string SecretKey, string QrCodeUri);