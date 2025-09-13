using FluentValidation;
using NettoRetail.Application.Models.Request.Review;

namespace NettoRetail.API.Infrastructure.Validators;

public class BulkRejectFeedbacksRequestValidator : AbstractValidator<BulkRejectFeedbacksRequest>
{
    public BulkRejectFeedbacksRequestValidator()
    {
        RuleFor(x => x.Reason)
            .NotEmpty().WithMessage("Please provide a reason for rejection.")
            .MaximumLength(500).WithMessage("Reason must be less than 500 characters.");

        RuleFor(x => x.FeedBackIds)
            .NotEmpty().WithMessage("At least one feedback id must be provided.");
    }
}
