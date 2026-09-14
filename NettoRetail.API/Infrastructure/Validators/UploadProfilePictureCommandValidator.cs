using FluentValidation;
using NettoRetail.Application.CQRS.Users.Commands;
using NettoRetail.Application.Models.Request;

namespace NettoRetail.API.Infrastructure.Validators
{
    public class UploadProfilePictureCommandValidator
     : AbstractValidator<UploadProfilePictureCommand>
    {
        public UploadProfilePictureCommandValidator(
            IValidator<UploadProfilePictureRequest> validator)
        {
            RuleFor(x => x.Model)
                .SetValidator(validator);
        }
    }
}
