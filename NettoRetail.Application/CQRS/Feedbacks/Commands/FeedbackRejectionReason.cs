using NettoRetail.Application.Exceptions;

namespace NettoRetail.Application.CQRS.Feedbacks.Commands;

internal static class FeedbackRejectionReason
{
    public const int MaxLength = 500;

    public static string NormalizeOrThrow(string? reason)
    {
        if (string.IsNullOrWhiteSpace(reason))
            throw new BadRequestException("Please provide a reason for rejection.");

        var trimmed = reason.Trim();
        if (trimmed.Length > MaxLength)
            throw new BadRequestException($"Reason must be less than {MaxLength} characters.");

        return trimmed;
    }
}
