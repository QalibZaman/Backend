using FluentValidation;
using NettoRetail.Application.Models.Request.Content;

namespace NettoRetail.API.Infrastructure.Validators;

public class UpdateContentRequestValidator : AbstractValidator<UpdateContentRequest>
{
    private const int MaxContentLength = 50 * 1024; // 50 KB

    public UpdateContentRequestValidator()
    {
        RuleFor(x => x.Content)
            .NotEmpty()
            .WithMessage("Content is required.")
            .MaximumLength(MaxContentLength)
            .WithMessage($"Content cannot exceed {MaxContentLength} characters.");
        RuleFor(x => x.IsVisible)
            .Must((content, isVisible) =>
                (!string.IsNullOrWhiteSpace(content.Content)) || isVisible == false)
            .WithMessage("Visibility must be false when content is empty or whitespace.");

    }
}
