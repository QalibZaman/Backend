using FluentValidation;
using NettoRetail.API.Infrastructure.Extensions;
using NettoRetail.API.Infrastructure.Validators.Methods;
using NettoRetail.Application.Requests.TeaserSection;

namespace NettoRetail.API.Infrastructure.Validators;

public class UpdateTeaserSectionCommandValidator : AbstractValidator<TeaserSectionRequest>
{
    public UpdateTeaserSectionCommandValidator()
    {
        RuleFor(x => x.Heading)
                .NotEmpty().WithMessage("Heading is required.")
                .MinimumLength(2).WithMessage("Heading must be at least 2 characters")
                .MaximumLength(60).WithMessage("Heading must be less than or equal to 60 characters")
                .Matches(@"^[A-Za-z0-9_-](?:[A-Za-z0-9 _-]*[A-Za-z0-9_-])?$").WithMessage("Heading can only contain letters, numbers, hyphens, underscores and not start or end with a space. Please check your input");

        RuleFor(x => x.AdditionalText)
                .MinimumLength(2).WithMessage("Additional text must be at least 2 characters")
                .MaximumLength(100).WithMessage("Additional text must be less than or equal to 100 characters")
                .Matches(@"^[A-Za-z0-9_-](?:[A-Za-z0-9 _-]*[A-Za-z0-9_-])?$").WithMessage("Additional text can only contain letters, numbers, hyphens, underscores and not start or end with a space. Please check your input");

        RuleFor(x => x.Link)
            .Must(UrlValidator.BeAValidHttpOrHttpsDomain)
                .WithMessage("Website URL must be a valid URL including protocol (e.g., https://)")
            .MaximumLength(100).WithMessage("Link must be less than or equal to 100 characters.");

        RuleFor(x => x.AdditionalTextColor)
            .Matches("^#([A-Fa-f0-9]{6})$")
                .WithMessage("Color must be a valid hex code (e.g. #CE6767).");

        RuleFor(x => x.HeadingColor)
            .Matches("^#([A-Fa-f0-9]{6})$")
                .WithMessage("Color must be a valid hex code (e.g. #CE6767).");
    }
}
