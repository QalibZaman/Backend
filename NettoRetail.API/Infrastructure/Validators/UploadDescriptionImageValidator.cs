using FluentValidation;

namespace NettoRetail.API.Infrastructure.Validators;
public class UploadDescriptionImageValidator : AbstractValidator<IFormFile>
{
    private const long MaxFileSize = 2 * 1024 * 1024;
    private readonly string[] allowedExtensions = [".jpg", ".jpeg", ".png", ".gif"];

    public UploadDescriptionImageValidator()
    {
        RuleFor(file => file.FileName)
                .Must(name => allowedExtensions.Contains(Path.GetExtension(name).ToLower()))
                .WithMessage("Unsupported file format. Please upload a JPEG, PNG, or GIF.");

        RuleFor(file => file.Length)
              .GreaterThan(0)
              .LessThanOrEqualTo(MaxFileSize)
              .WithMessage("File size too large. Maximum file size is 2 MB.");
    }
}
