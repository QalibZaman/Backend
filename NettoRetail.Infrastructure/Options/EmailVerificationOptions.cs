using System.ComponentModel.DataAnnotations;

namespace NettoRetail.Infrastructure.Options;

public class EmailVerificationOptions
{
    public int TokenExpirationHours { get; init; }
    [Required]
    public required string VerificationLink { get; init; }
}
