namespace NettoRetail.Application.Models.Request;

public record ChangePasswordRequest(string CurrentPassword, string NewPassword, string ConfirmPassword);
