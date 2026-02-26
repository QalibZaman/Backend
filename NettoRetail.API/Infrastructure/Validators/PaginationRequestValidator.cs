using FluentValidation;
using NettoRetail.Application.Models.Request;
using NettoRetail.Application.Utilities;

namespace NettoRetail.API.Infrastructure.Validators;

public class PaginationRequestValidator : AbstractValidator<PaginationRequest>
{
    public PaginationRequestValidator()
    {
        RuleFor(model => model.PageNumber).GreaterThanOrEqualTo(1);
        RuleFor(model => model.PageSize).GreaterThanOrEqualTo(1);
        RuleFor(model => model)
            .Must(model => PaginationHelper.IsSkipWithinIntRange(model.PageNumber, model.PageSize))
            .WithMessage("Pagination parameters are too large.");
    }
}
