using FluentValidation;
using NettoRetail.API.Infrastructure.Extensions;
using NettoRetail.Application.Models.Request.Brand;
using NettoRetail.Shared.Constants;

namespace NettoRetail.API.Infrastructure.Validators;

public class CreateBrandRequestValidator : AbstractValidator<CreateBrandRequest>
{
    private const long MaxFileSize = 5 * 1024 * 1024; // 5 MB

    public CreateBrandRequestValidator()
    {
        RuleFor(x => x.Name)
            .NotEmpty()
            .Length(2, 90)
            .Matches(@"^[a-zA-Z0-9 \-_]+$").WithMessage(ValidationErrorMessages.Brand.InvalidBrandNameErrorMessage)
            .Must(name => !name.StartsWith(' ') && !name.EndsWith(' '))
                .WithMessage(ValidationErrorMessages.Brand.InvalidBrandNameErrorMessage);

        RuleFor(x => x.Description)
            .MaximumLength(500)
            .Matches(@"^[a-zA-Z0-9 .,!?\-_'""()]+$").When(x => !string.IsNullOrEmpty(x.Description))
                .WithMessage(ValidationErrorMessages.Brand.InvalidBrandDescriptionErrorMessage);

        RuleFor(x => x.WebsiteUrl)
            .MustBeAValidUrl().When(x => !string.IsNullOrEmpty(x.WebsiteUrl))
                .WithMessage(ValidationErrorMessages.Website.InvalidWebsiteUrlErrorMessage)
            .MaximumLength(100).When(x => !string.IsNullOrEmpty(x.WebsiteUrl));

        When(x => x.LogoFile != null, () =>
        {
            RuleFor(x => x.LogoFile!.FileName)
                .MustHaveImageExtension()
                .WithMessage(ValidationErrorMessages.File.InvalidFileSizeErrorMessage);

            RuleFor(x => x.LogoFile!.Length)
                .LessThanOrEqualTo(MaxFileSize)
                .WithMessage(ValidationErrorMessages.File.InvalidFileSizeErrorMessage);
        });
    }
}
