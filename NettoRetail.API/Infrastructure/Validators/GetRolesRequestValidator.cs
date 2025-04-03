using FluentValidation;
using NettoRetail.Application.Models.Request.Role;

namespace NettoRetail.API.Infrastructure.Validators;

public class GetRolesRequestValidator : AbstractValidator<GetRolesRequest>
{
    public GetRolesRequestValidator()
    {
        RuleFor(model => model.PageNumber).GreaterThanOrEqualTo(0);
        RuleFor(model => model.PageSize).GreaterThanOrEqualTo(1);
    }
}
