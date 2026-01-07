namespace NettoRetail.Infrastructure.Options;

public class EmailVerificationOptions
{
    public int TokenExpirationHours { get; init; }
    public required string VerificationLink { get; init; }
}
