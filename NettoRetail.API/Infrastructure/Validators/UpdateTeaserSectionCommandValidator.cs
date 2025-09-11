using FluentValidation;
using NettoRetail.Application.CQRS.TeaserSection.Commands;

namespace NettoRetail.API.Infrastructure.Validators;

public class UpdateTeaserSectionCommandValidator : AbstractValidator<UpdateTeaserSectionCommand>
{
    public UpdateTeaserSectionCommandValidator()
    {
        RuleFor(x => x.Model.Heading)
                .NotEmpty().WithMessage("Heading is required.")
                .MinimumLength(2).WithMessage("Heading must be at least 2 characters")
                .MaximumLength(60).WithMessage("Heading must be less than or equal to 60 characters")
                .Matches(@"^[A-Za-z0-9_-](?:[A-Za-z0-9 _-]*[A-Za-z0-9_-])?$").WithMessage("Heading can only contain letters, numbers, hyphens, underscores and not start or end with a space. Please check your input");

        RuleFor(x => x.Model.AdditionalText)
                .MinimumLength(2).WithMessage("Additional text must be at least 2 characters")
                .MaximumLength(60).WithMessage("Additional text must be less than or equal to 60 characters")
                .Matches(@"^[A-Za-z0-9_-](?:[A-Za-z0-9 _-]*[A-Za-z0-9_-])?$").WithMessage("Additional text can only contain letters, numbers, hyphens, underscores and not start or end with a space. Please check your input");

        RuleFor(x => x.Model.Link)
            .MaximumLength(100).WithMessage("Link must be less than or equal to 100 characters.");
    }
}
