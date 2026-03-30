namespace NettoRetail.Infrastructure.Options;

public class PlatformAdminActivationOptions
{
    public string ActivationLink { get; set; } = string.Empty;
    public int TokenExpirationHours { get; set; } = 24;
}
