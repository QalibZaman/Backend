using FluentValidation;
using NettoRetail.Application.Models.Request.Brand;

namespace NettoRetail.API.Infrastructure.Validators;

public class UpdateBrandRequestValidator : AbstractValidator<UpdateBrandRequest>
{
    private readonly IValidator<CreateBrandRequest> _createBrandRequestValidator;

    public UpdateBrandRequestValidator(IValidator<CreateBrandRequest> createBrandRequestValidator)
    {
        _createBrandRequestValidator = createBrandRequestValidator;

        RuleFor(x => x).SetValidator(_createBrandRequestValidator);
    }
}