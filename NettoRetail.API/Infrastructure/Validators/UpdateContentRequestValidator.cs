using FluentValidation;
using NettoRetail.Application.Utilities;
using NettoRetail.Application.Models.Request.Content;
using NettoRetail.Shared.Constants;

namespace NettoRetail.API.Infrastructure.Validators;

public class UpdateContentRequestValidator : AbstractValidator<UpdateContentRequest>
{
    public UpdateContentRequestValidator()
    {
        RuleFor(x => x)
            .Custom((request, context) =>
            {
                var sanitizedContent = ContentBlockRules.Sanitize(request.Content);

                if (!ContentBlockRules.HasMeaningfulContent(sanitizedContent))
                {
                    context.AddFailure(nameof(request.Content), ValidationErrorMessages.Content.IsRequired);
                    return;
                }

                if (sanitizedContent.Length > ValidationErrorMessages.Content.MaxContentLength)
                {
                    context.AddFailure(
                        nameof(request.Content),
                        ValidationErrorMessages.Content.MaxContentLengthMessage
                            .Replace("{MaxLength}", ValidationErrorMessages.Content.MaxContentLength.ToString()));
                }

                var imageValidationError = ContentBlockRules.ValidateImages(sanitizedContent);
                if (imageValidationError != null)
                {
                    context.AddFailure(nameof(request.Content), imageValidationError);
                }
            });
    }
}
