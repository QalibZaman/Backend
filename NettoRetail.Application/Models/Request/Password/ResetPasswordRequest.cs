namespace NettoRetail.Application.Models.Request.Password
{
    public record ResetPasswordRequest(string Token, string NewPassword, string ConfirmPassword);
}
