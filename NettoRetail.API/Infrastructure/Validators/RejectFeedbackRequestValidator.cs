using FluentValidation;
using NettoRetail.Application.Models.Feedback;

namespace NettoRetail.API.Infrastructure.Validators;

public class RejectFeedbackRequestValidator : AbstractValidator<RejectFeedbackRequest>
{
    public RejectFeedbackRequestValidator()
    {
        RuleFor(x => x.Reason)
            .NotEmpty().WithMessage("Please provide a reason for rejection.")
            .Must(reason => !string.IsNullOrWhiteSpace(reason)).WithMessage("Please provide a reason for rejection.")
            .MaximumLength(500).WithMessage("Reason must be less than 500 characters.");
    }
}
