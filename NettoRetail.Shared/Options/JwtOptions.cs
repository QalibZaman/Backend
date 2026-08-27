namespace NettoRetail.Shared.Options;

public class JwtOptions
{
    public required string SecretKey { get; set; }
    public required double AccessTokenExpirationMinutes { get; set; }
    public required double RefreshTokenExpirationDays { get; set; }
    public required double TempTokenExpirationMinutes { get; set; }
}
