using FluentValidation;
using NettoRetail.Application.CQRS.Users.Commands.Staff;
using NettoRetail.Application.Models.Request.Staff;

namespace NettoRetail.API.Infrastructure.Validators
{
    public class UpdateStaffCommandValidator : AbstractValidator<UpdateStaffCommand>
    {
        private readonly IValidator<UpdateStaffRequest> _validator;

        public UpdateStaffCommandValidator(IValidator<UpdateStaffRequest> validator)
        {
            _validator = validator;
            ConfigureRules();
        }

        private void ConfigureRules()
        {
            RuleFor(x => x.Id)
                .NotEmpty();

            RuleFor(x => x.ShopId)
                .GreaterThan(0);

            RuleFor(x => x.Model)
                .SetValidator(_validator);
        }
    }
}
