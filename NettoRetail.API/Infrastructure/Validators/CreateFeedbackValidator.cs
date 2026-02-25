using FluentValidation;
using NettoRetail.Application.Models.Feedback;

namespace NettoRetail.API.Infrastructure.Validators;

public class CreateFeedbackValidator : AbstractValidator<CreateFeedbackModel>
{
    private const string AllowedTextRegex =
        @"^(?!\s)[A-Za-z0-9\s!@#$%^&*()_\-+={}\[\]:;""'<>?,./\\|`~]+(?<!\s)$";

    public CreateFeedbackValidator()
    {
        RuleFor(x => x.Rating)
            .InclusiveBetween(1, 5)
            .WithMessage("Rating must be between 1 and 5.");

        RuleFor(x => x.Title)
            .MaximumLength(60)
            .WithMessage("Title cannot be longer than 60 characters.")
            .Matches(AllowedTextRegex)
            .When(x => !string.IsNullOrWhiteSpace(x.Title))
            .WithMessage("In title allowed: Latin letters, digits, special characters. No leading/trailing spaces.");

        RuleFor(x => x.Text)
            .MaximumLength(500)
            .WithMessage("Text cannot be longer than 500 characters.")
            .Matches(AllowedTextRegex)
            .When(x => !string.IsNullOrWhiteSpace(x.Text))
            .WithMessage("In Text allowed: Latin letters, digits, special characters. No leading/trailing spaces.");
    }
}