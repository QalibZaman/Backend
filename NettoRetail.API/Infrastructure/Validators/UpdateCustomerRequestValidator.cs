using FluentValidation;
using NettoRetail.Application.Models.Request.Customer;

namespace NettoRetail.API.Infrastructure.Validators;

public class UpdateCustomerRequestValidator : AbstractValidator<UpdateCustomerRequest>
{
    private readonly IValidator<CreateCustomerRequest> _validator;
    public UpdateCustomerRequestValidator(IValidator<CreateCustomerRequest> validator)
    {
        _validator = validator;

        RuleFor(x => x)
            .SetValidator(_validator);
    }
}
