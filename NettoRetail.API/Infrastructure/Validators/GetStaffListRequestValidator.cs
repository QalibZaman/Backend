using FluentValidation;
using NettoRetail.Application.Models.Request;

namespace NettoRetail.API.Infrastructure.Validators;

public class GetStaffListRequestValidator : AbstractValidator<GetStaffListRequest>
{
    public GetStaffListRequestValidator()
    {
        RuleFor(model => model.PageNumber).GreaterThanOrEqualTo(0);
        RuleFor(model => model.PageSize).GreaterThanOrEqualTo(1);
    }
}
