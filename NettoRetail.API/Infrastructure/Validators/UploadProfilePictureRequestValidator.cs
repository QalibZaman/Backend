using FluentValidation;
using NettoRetail.Application.Models.Request;

namespace NettoRetail.API.Infrastructure.Validators
{
    public class UploadProfilePictureRequestValidator : AbstractValidator<UploadProfilePictureRequest>
    {
        public UploadProfilePictureRequestValidator()
        {
            RuleFor(x => x.File)
                .NotNull()
                .WithMessage("Profile picture is required.");

            RuleFor(x => x.File.Length)
                .GreaterThan(0)
                .When(x => x.File != null)
                .WithMessage("Profile picture is required.");
        }
    }
}
