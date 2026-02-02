using FluentValidation;
using NettoRetail.Application.Models.Request.Content;

namespace NettoRetail.API.Infrastructure.Validators
{
    public class UpdateContentRequestValidator : AbstractValidator<UpdateContentRequest>
    {
        public UpdateContentRequestValidator()
        {
            RuleFor(x => x.Content)
            .MaximumLength(50000)
                .WithMessage("Content must not exceed 50,000 characters.");

            RuleFor(x => x.IsVisible)
           .Must((content, isVisible) =>
               (!string.IsNullOrWhiteSpace(content.Content)) || isVisible == false)
           .WithMessage("Visibility must be false when content is empty or whitespace.");
        }
    }
}
