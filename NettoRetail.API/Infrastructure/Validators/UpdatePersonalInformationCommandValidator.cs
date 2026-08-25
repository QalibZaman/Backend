using FluentValidation;
using NettoRetail.Application.CQRS.Users.Commands.PersonalInformation;
using NettoRetail.Application.Models.Request;

namespace NettoRetail.API.Infrastructure.Validators
{
    public class UpdatePersonalInformationCommandValidator
    : AbstractValidator<UpdatePersonalInformationCommand>
    {
        public UpdatePersonalInformationCommandValidator(
            IValidator<UpdatePersonalInformationRequest> validator)
        {
            ConfigureRules(validator);
        }

        private void ConfigureRules(
            IValidator<UpdatePersonalInformationRequest> validator)
        {
            RuleFor(x => x.Model)
                .SetValidator(validator);
        }
    }
}
