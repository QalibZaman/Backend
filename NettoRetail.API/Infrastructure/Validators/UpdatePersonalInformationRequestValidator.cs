using FluentValidation;
using NettoRetail.Application.Models.Request;

namespace NettoRetail.API.Infrastructure.Validators
{
    public class UpdatePersonalInformationRequestValidator
      : AbstractValidator<UpdatePersonalInformationRequest>
    {
        public UpdatePersonalInformationRequestValidator()
        {
            RuleFor(x => x.FirstName)
                .NotEmpty()
                .MaximumLength(100);

            RuleFor(x => x.LastName)
                .NotEmpty()
                .MaximumLength(100);

            RuleFor(x => x.Email)
                .NotEmpty()
                .EmailAddress()
                .MaximumLength(255);

            RuleFor(x => x.PhoneNumber)
                .MaximumLength(30)
                .When(x => !string.IsNullOrWhiteSpace(x.PhoneNumber));

            RuleFor(x => x.Street)
                .MaximumLength(255);

            RuleFor(x => x.HouseNumber)
                .MaximumLength(50);

            RuleFor(x => x.Apartment)
                .MaximumLength(50);

            RuleFor(x => x.City)
                .MaximumLength(100);

            RuleFor(x => x.State)
                .MaximumLength(100);

            RuleFor(x => x.ZipCode)
                .MaximumLength(20);

            RuleFor(x => x.CountryId)
                .GreaterThan(0)
                .When(x => x.CountryId.HasValue);
        }
    }
}
