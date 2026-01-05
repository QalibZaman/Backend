namespace NettoRetail.Infrastructure.Options;

public class EmailVerificationOptions
{
    public int TokenExpirationHours { get; init; }
    public string? VerificationLink { get; init; }
}
