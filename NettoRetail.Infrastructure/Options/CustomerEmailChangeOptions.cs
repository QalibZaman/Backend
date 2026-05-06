namespace NettoRetail.Infrastructure.Options;

public class CustomerEmailChangeOptions
{
    public int TokenExpirationHours { get; init; }
    public required string EmailChangeLink { get; init; }
}
