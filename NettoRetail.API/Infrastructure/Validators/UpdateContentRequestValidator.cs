using FluentValidation;
using NettoRetail.Application.Models.Request.Content;
using NettoRetail.Shared.Constants;

namespace NettoRetail.API.Infrastructure.Validators;

public class UpdateContentRequestValidator : AbstractValidator<UpdateContentRequest>
{

    public UpdateContentRequestValidator()
    {
        RuleFor(x => x.Content)
            .NotEmpty()
            .WithMessage(ValidationErrorMessages.Content.IsRequired)
            .MaximumLength(ValidationErrorMessages.Content.MaxContentLength)
            .WithMessage(ValidationErrorMessages.Content.MaxContentLengthMessage)
            .When(x => x.IsVisible);

        RuleFor(x => x.IsVisible)
            .Must((content, isVisible) => !isVisible || !string.IsNullOrWhiteSpace(content.Content))
            .WithMessage(ValidationErrorMessages.Content.ContentVisibilityRequiredMessage);

    }
}
