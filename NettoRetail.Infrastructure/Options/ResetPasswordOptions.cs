namespace NettoRetail.Infrastructure.Options;

public class ResetPasswordOptions
{
    public string ResetLink { get; set; } = string.Empty;
    public int TokenExpirationHours { get; set; }
    public int ActivationTokenExpirationHours { get; set; }
}
