namespace NettoRetail.Infrastructure.Options
{
    public class CustomerPasswordResetOptions
    {
        public int TokenExpirationHours { get; init; }
        public required string ResetPasswordLink { get; init; }
    }
}
