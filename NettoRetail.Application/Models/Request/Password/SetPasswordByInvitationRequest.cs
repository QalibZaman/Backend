namespace NettoRetail.Application.Models.Request.Password;

public sealed class SetPasswordByInvitationRequest
{
    public string Token { get; set; } = default!;
    public string Password { get; set; } = default!;
    public string ConfirmPassword { get; set; } = default!;
}