using FluentValidation;
using NettoRetail.Application.Models.Request;

namespace NettoRetail.API.Infrastructure.Validators;

public class PaginationRequestValidator : AbstractValidator<PaginationRequest>
{
    public PaginationRequestValidator()
    {
        RuleFor(model => model.PageNumber).GreaterThanOrEqualTo(1);
        RuleFor(model => model.PageSize).GreaterThanOrEqualTo(1);
    }
}
