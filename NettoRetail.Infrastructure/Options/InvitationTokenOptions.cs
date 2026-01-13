namespace NettoRetail.Infrastructure.Options;

public class FrontendOptions
{
    public string BaseUrl { get; set; } = string.Empty;
}

public class InvitationOptions
{
    public int PlatformAdminExpiryHours { get; set; } = 48;
}