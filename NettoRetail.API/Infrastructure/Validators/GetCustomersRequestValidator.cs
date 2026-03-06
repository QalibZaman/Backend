using FluentValidation;
using NettoRetail.Application.Models.Request.Customer;

namespace NettoRetail.API.Infrastructure.Validators;

public class GetCustomersRequestValidator : AbstractValidator<GetCustomersRequest>
{
    public GetCustomersRequestValidator()
    {
        RuleFor(x => x.Search)
            .MaximumLength(256)
            .WithMessage("Search length must be less than or equal to 256 characters.");
    }
}
