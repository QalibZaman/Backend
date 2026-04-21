using FluentValidation;
using NettoRetail.Application.Utilities;
using NettoRetail.Shared.Constants;

namespace NettoRetail.Application.CQRS.Contents.Queries;

public class PreviewContentBySlugQueryValidator : AbstractValidator<PreviewContentBySlugQuery>
{
    public PreviewContentBySlugQueryValidator()
    {
        RuleFor(x => x.Model)
            .NotNull()
            .Custom((model, context) =>
            {
                if (model is null)
                {
                    return;
                }

                var sanitizedContent = ContentBlockRules.Sanitize(model.Content);

                if (!ContentBlockRules.HasMeaningfulContent(sanitizedContent))
                {
                    context.AddFailure("Model.Content", ValidationErrorMessages.Content.IsRequired);
                    return;
                }

                var imageValidationError = ContentBlockRules.ValidateImages(sanitizedContent);
                if (imageValidationError is not null)
                {
                    context.AddFailure("Model.Content", imageValidationError);
                }
            });
    }
}
