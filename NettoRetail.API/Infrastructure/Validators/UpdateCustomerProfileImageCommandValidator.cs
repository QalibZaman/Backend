using FluentValidation;
using NettoRetail.API.Infrastructure.Extensions;
using NettoRetail.Application.CQRS.Customers.Commands;

namespace NettoRetail.API.Infrastructure.Validators
{
    public class UpdateCustomerProfileImageCommandValidator : AbstractValidator<UpdateCustomerProfileImageCommand>
    {
        private const long MaxFileSize = 1 * 1024 * 1024; // 1 MB
        public UpdateCustomerProfileImageCommandValidator()
        {
            ConfigureRules();
        }

        private void ConfigureRules()
        {
            RuleLevelCascadeMode = CascadeMode.Stop;

            RuleFor(x => x.UserId)
                .NotEmpty();

            RuleFor(x => x.Image)
                .NotEmpty()
                .Must(image => image.Length > 0)
                .WithMessage("Image file cannot be empty");

            RuleFor(x => x.Image.FileName)
                .MustHaveImageExtension()
                .WithMessage("Unsupported file format. Only JPG, JPEG, PNG allowed.");

            RuleFor(x => x.Image.Length)
                .LessThanOrEqualTo(MaxFileSize)
                .WithMessage($"Image file size cannot exceed {MaxFileSize / (1024 * 1024)} MB");
        }
    }
}
