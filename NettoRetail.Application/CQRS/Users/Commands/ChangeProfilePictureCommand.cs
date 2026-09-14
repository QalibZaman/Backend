using MediatR;
using NettoRetail.Application.Models.Request;

namespace NettoRetail.Application.CQRS.Users.Commands
{
    public record ChangeProfilePictureCommand(UploadProfilePictureRequest Model, Guid UserId) : IRequest;
}
