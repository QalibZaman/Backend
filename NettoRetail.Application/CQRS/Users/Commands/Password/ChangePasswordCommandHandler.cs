using MediatR;
using NettoRetail.Application.Models.Request.Password;
using NettoRetail.Application.Services.IPasswordService;

namespace NettoRetail.Application.CQRS.Users.Commands.Password;

public record ChangePasswordCommand(Guid UserId, ChangePasswordRequest Model) : IRequest;

public class ChangePasswordCommandHandler(
    IPasswordService passwordService)
    : IRequestHandler<ChangePasswordCommand>
{
    public async Task Handle(ChangePasswordCommand request, CancellationToken cancellationToken)
    {
        await passwordService.ChangePasswordAsync(
            request.UserId,
            request.Model.CurrentPassword,
            request.Model.NewPassword,
            cancellationToken);
    }
}
