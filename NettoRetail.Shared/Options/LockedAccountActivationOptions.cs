namespace NettoRetail.Infrastructure.Options;

public class LockedAccountActivationOptions
{
    public int TokenExpirationHours { get; init; }
    public required string ActivationLink { get; init; }
}