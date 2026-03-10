using FluentValidation;
using NettoRetail.Application.Models.Request.Content;

namespace NettoRetail.API.Infrastructure.Validators;

public class UpdateContentRequestValidator : AbstractValidator<UpdateContentRequest>
{

    public UpdateContentRequestValidator()
    {
        RuleFor(x => x.Content)
            .NotEmpty()
            .WithMessage(Constants.Validation.Content.IsRequired)
            .MaximumLength(Constants.Validation.Content.MaxContentLength)
            .WithMessage(Constants.Validation.Content.MaxContentLengthMessage)
            .When(x => x.IsVisible);

        RuleFor(x => x.IsVisible)
            .Must((content, isVisible) => !isVisible || !string.IsNullOrWhiteSpace(content.Content))
            .WithMessage(Constants.Validation.Content.ContentVisibilityRequiredMessage);
    }
}
