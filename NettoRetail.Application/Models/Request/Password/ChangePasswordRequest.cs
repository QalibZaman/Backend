namespace NettoRetail.Application.Models.Request.Password
{
    public record ChangePasswordRequest(string CurrentPassword, string NewPassword, string ConfirmPassword);
}
