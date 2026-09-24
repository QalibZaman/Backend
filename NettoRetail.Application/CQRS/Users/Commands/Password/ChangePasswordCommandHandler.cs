using MediatR;
using Microsoft.AspNetCore.Identity;
using NettoRetail.Application.Exceptions;
using NettoRetail.Application.Models.Request.Password;
using NettoRetail.DataAccess.Identity;

namespace NettoRetail.Application.CQRS.Users.Commands.Password;

public record ChangePasswordCommand(Guid UserId, ChangePasswordRequest Model) : IRequest;

public class ChangePasswordCommandHandler(UserManager<ApplicationUser> userManager)
    : IRequestHandler<ChangePasswordCommand>
{
    public async Task Handle(ChangePasswordCommand request, CancellationToken cancellationToken)
    {
        var identityUser = await userManager.FindByIdAsync(request.UserId.ToString());
        if (identityUser is null)
        {
            throw new NotFoundException("User not found.");
        }

        var result =
            await userManager.ChangePasswordAsync(identityUser, request.Model.CurrentPassword,
                request.Model.NewPassword);
        if (!result.Succeeded)
        {
            throw new BadRequestException(string.Join(", ", result.Errors.Select(e => e.Description)));
        }
    }
}