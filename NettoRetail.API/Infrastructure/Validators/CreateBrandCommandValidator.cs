using FluentValidation;
using NettoRetail.Application.CQRS.Brands.Commands;
using NettoRetail.Application.Models.Request.Brand;
using NettoRetail.Application.Services.IBrandService;

namespace NettoRetail.API.Infrastructure.Validators;

public class CreateBrandCommandValidator : AbstractValidator<CreateBrandCommand>
{
    private readonly IBrandService _brandService;
    public CreateBrandCommandValidator(IBrandService brandService)
    {
        _brandService = brandService;
        ConfigureRules();
    }

    private void ConfigureRules()
    {
        ClassLevelCascadeMode = CascadeMode.Stop;

        RuleFor(x => x.ShopId)
            .GreaterThan(0);

        RuleFor(x => x)
            .CustomAsync(ValidateBrandCreationAsync);
    }

    private async Task ValidateBrandCreationAsync(CreateBrandCommand request, ValidationContext<CreateBrandCommand> _, CancellationToken cancellationToken)
    {
        await _brandService.EnsureBrandCanBeCreatedAsync(request.ShopId, request.Model.Name, request.Model.WebsiteUrl, cancellationToken);
    }
}
