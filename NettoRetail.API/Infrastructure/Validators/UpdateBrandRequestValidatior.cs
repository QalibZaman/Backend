using FluentValidation;
using NettoRetail.API.Infrastructure.Extensions;

public class UpdateBrandRequestValidator : AbstractValidator<UpdateBrandRequest>
{
    private const long MaxFileSize = 5 * 1024 * 1024; // 5 MB

    public UpdateBrandRequestValidator()
    {
        RuleFor(x => x.Name)
           .NotEmpty().WithMessage("Brand name is required")
           .Length(2, 50).WithMessage("Brand name must be between 2 and 50 characters")
           .Matches(@"^[a-zA-Z0-9 \-_]+$").WithMessage("Brand name contains invalid characters")
           .Must(name => !name.StartsWith(' ') && !name.EndsWith(' '))
               .WithMessage("Brand name cannot start or end with a space");

        RuleFor(x => x.Description)
            .MaximumLength(500).WithMessage("Brand description cannot exceed 500 characters")
            .Matches(@"^[a-zA-Z0-9 .,!?\-_'""()]+$").When(x => !string.IsNullOrEmpty(x.Description))
                .WithMessage("Brand description contains invalid characters");

        RuleFor(x => x.WebsiteUrl)
            .MustBeAValidUrl().When(x => !string.IsNullOrEmpty(x.WebsiteUrl))
                .WithMessage("Website URL must be a valid URL including protocol (e.g., https://)")
            .MaximumLength(100).When(x => !string.IsNullOrEmpty(x.WebsiteUrl))
                .WithMessage("Website URL cannot exceed 100 characters");

        When(x => x.Logo != null, () =>
        {
            RuleFor(x => x.Logo!.FileName)
                .MustHaveImageExtension()
                .WithMessage("Unsupported file format. Only JPG, JPEG, PNG, or WEBP allowed.");

            RuleFor(x => x.Logo!.Length)
                .LessThanOrEqualTo(MaxFileSize)
                .WithMessage("File size must be 5MB or less.");
        });
    }
}
