using FluentValidation;
using NettoRetail.Application.Models.Request.Brand;

namespace NettoRetail.API.Infrastructure.Validators;

public class CreateBrandRequestValidator : AbstractValidator<CreateBrandRequest>
{
    private const long MaxFileSize = 5 * 1024 * 1024; // 5 MB
    private static readonly string[] PermittedExtensions = [".jpg", ".jpeg", ".png", ".webp"];

    public CreateBrandRequestValidator()
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
            .Must(BeValidUrl).When(x => !string.IsNullOrEmpty(x.WebsiteUrl))
                .WithMessage("Website URL must be a valid URL including protocol (e.g., https://)")
            .MaximumLength(100).When(x => !string.IsNullOrEmpty(x.WebsiteUrl))
                .WithMessage("Website URL cannot exceed 100 characters");

        When(x => x.LogoFile != null, () =>
        {
            RuleFor(x => x.LogoFile!.FileName)
                .Must(HaveAllowedExtension)
                .WithMessage("Unsupported file format. Only JPG, JPEG, PNG, or WEBP allowed.");

            RuleFor(x => x.LogoFile!.Length)
                .LessThanOrEqualTo(MaxFileSize)
                .WithMessage("File size must be 5MB or less.");
        });
    }

    private bool BeValidUrl(string url)
    {
        return Uri.TryCreate(url, UriKind.Absolute, out var uriResult)
               && (uriResult.Scheme == Uri.UriSchemeHttp || uriResult.Scheme == Uri.UriSchemeHttps);
    }

    private bool HaveAllowedExtension(string fileName)
    {
        var extension = Path.GetExtension(fileName).ToLowerInvariant();
        return PermittedExtensions.Contains(extension);
    }
}
