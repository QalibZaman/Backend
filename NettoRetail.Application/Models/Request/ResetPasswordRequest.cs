namespace NettoRetail.Application.Models.Request;

public record ResetPasswordRequest(string Token, string NewPassword, string ConfirmPassword);
