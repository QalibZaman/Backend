using FluentValidation;
using NettoRetail.API.Infrastructure.Validators.Methods;
using NettoRetail.Application.Requests.TeaserSection;
using NettoRetail.Shared.Constants;

namespace NettoRetail.API.Infrastructure.Validators;

public class CreateTeaserSectionCommandValidator : AbstractValidator<CreateTeaserSectionRequest>
{
    public CreateTeaserSectionCommandValidator()
    {
        RuleFor(x => x.Heading)
            .NotEmpty().WithMessage(ValidationErrorMessages.TeaserSection.HeadingEmptyErrorMessage)
            .MinimumLength(2).WithMessage(string.Format(ValidationErrorMessages.TeaserSection.HeadingMinimumLengthErrorMessage, 2))
            .MaximumLength(60).WithMessage(string.Format(ValidationErrorMessages.TeaserSection.HeadingMaximumLengthErrorMessage, 60))
            .Matches(@"^[A-Za-z0-9_-](?:[A-Za-z0-9 _-]*[A-Za-z0-9_-])?$").WithMessage(ValidationErrorMessages.TeaserSection.HeadingInvalidFormatErrorMessage);

        RuleFor(x => x.AdditionalText)
            .MinimumLength(2).WithMessage(string.Format(ValidationErrorMessages.TeaserSection.AdditionalTextMinimumLengthErrorMessage, 2))
            .MaximumLength(100).WithMessage(string.Format(ValidationErrorMessages.TeaserSection.AdditionalTextMaximumLengthErrorMessage, 100))
            .Matches(@"^[A-Za-z0-9_-](?:[A-Za-z0-9 _-]*[A-Za-z0-9_-])?$").WithMessage(ValidationErrorMessages.TeaserSection.AdditionalTextInvalidFormatErrorMessage);

        RuleFor(x => x.Link)
            .Must(UrlValidator.BeAValidHttpOrHttpsDomain).WithMessage(ValidationErrorMessages.TeaserSection.LinkInvalidFormatErrorMessage)
            .MaximumLength(100).WithMessage(string.Format(ValidationErrorMessages.TeaserSection.LinkMaximumLengthErrorMessage, 100));

        RuleFor(x => x.AdditionalTextColor)
            .Matches("^#([A-Fa-f0-9]{6})$")
            .WithMessage(ValidationErrorMessages.TeaserSection.ColorInvalidFormatErrorMessage);

        RuleFor(x => x.HeadingColor)
            .Matches("^#([A-Fa-f0-9]{6})$")
            .WithMessage(ValidationErrorMessages.TeaserSection.ColorInvalidFormatErrorMessage);
    }
}