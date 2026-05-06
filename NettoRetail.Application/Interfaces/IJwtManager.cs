using System.Security.Claims;
using NettoRetail.Application.Models.Response;

namespace NettoRetail.Application.Interfaces;

public interface IJwtManager
{
    string Generate(List<Claim> claims, TimeSpan expiration);
    string GenerateWihoutTime(List<Claim> claims);
    ClaimsPrincipal? Validate(string refreshToken);
    ClaimsPrincipal ValidateResetToken(string resetToken);
    ClaimsPrincipal ValidateChangeEmailToken(string token);
    string GenerateTempToken(string userId);
    ClaimsPrincipal ValidateEmailActivation(string emailActivationToken);
    ClaimsPrincipal ValidatePlatformAdminActivationToken(string activationToken);
    ClaimsPrincipal ValidatePlatformAdminActivationTokenAllowExpired(string activationToken);
    ClaimsPrincipal ValidatePlatformAdminResetPasswordToken(string resetToken);
    TokenResponse GetTokenResponse(string userId, string userEmail, string userRoleId, int? shopId);
    ClaimsPrincipal ValidateCustomerResetPasswordToken(string customerResetPasswordToken);
}
