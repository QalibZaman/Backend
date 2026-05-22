using FluentValidation;
using NettoRetail.Application.CQRS.ProductReviews.Commands;

namespace NettoRetail.API.Infrastructure.Validators
{
    public class UploadProductReviewImageValidator : AbstractValidator<UploadProductReviewImageCommand>
    {
        private const long MaxFileSize = 5 * 1024 * 1024;
        private readonly string[] allowedExtensions = [".jpg", ".jpeg", ".png", ".gif"];

        public UploadProductReviewImageValidator()
        {
            RuleFor(file => file.Image.FileName)
                    .Must(name => allowedExtensions.Contains(Path.GetExtension(name).ToLower()))
                    .WithMessage("Unsupported file format. Please upload a JPEG, PNG, or GIF.");

            RuleFor(file => file.Image.Length)
                  .GreaterThan(0)
                  .LessThanOrEqualTo(MaxFileSize)
                  .WithMessage("File size too large. Maximum file size is 5 MB.");
        }
    }
}
