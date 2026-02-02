namespace NettoRetail.Shared.Options;

public class NewsletterOptions
{
    public int UnsubscribeTokenExpirationHours { get; set; }
    public string NewsletterLink { get; set; } = string.Empty;
}