using MediatR;

namespace NettoRetail.Application.CQRS.Users.Commands
{
    public record DeleteProfilePictureCommand : IRequest;
}
