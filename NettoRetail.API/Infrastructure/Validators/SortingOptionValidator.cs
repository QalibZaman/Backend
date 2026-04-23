using FluentValidation;
using NettoRetail.Application.Models.Request;
using NettoRetail.Shared.Constants;

namespace NettoRetail.API.Infrastructure.Validators;

public class SortingOptionValidator : AbstractValidator<SortingOption>
{
    public SortingOptionValidator()
    {
        RuleFor(x => x.Field)
            .NotEmpty()
            .WithMessage(ValidationErrorMessages.Sort.SortingFieldRequiredMessage);
    }
}
