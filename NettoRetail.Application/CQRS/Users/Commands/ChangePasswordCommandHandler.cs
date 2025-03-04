using MediatR;
using NettoRetail.Application.Exceptions;
using NettoRetail.Application.Models.Request;
using NettoRetail.DataAccess.Specifications;
using NettoRetail.Infrastructure.Interfaces;
using NettoRetail.Infrastructure.Models;
using NettoRetail.Shared.Interfaces.Repositories;
using NettoRetail.Shared.Utilities;

namespace NettoRetail.Application.CQRS.Users.Commands;

public record ChangePasswordCommand(Guid UserId, ChangePasswordRequest Model) : IRequest;

public class ChangePasswordCommandHandler(
    IStaffRepository staffRepository,
    IEmailSender emailSender)
    : IRequestHandler<ChangePasswordCommand>
{
    public async Task Handle(ChangePasswordCommand request, CancellationToken cancellationToken)
    {
        var specification = new GetStaffByIdSpecification(request.UserId);

        var user = await staffRepository
            .GetAsync(specification, cancellationToken) ??
            throw new NotFoundException("User not found.");

        var isCurrentPasswordCorrect = PasswordHasher.VerifyPassword(
            request.Model.CurrentPassword,
            user.PasswordHash);

        if (!isCurrentPasswordCorrect)
        {
            throw new BadRequestException("Password is not correct");
        }

        var isNewPasswordSameAsCurrent = PasswordHasher.VerifyPassword(
            request.Model.NewPassword,
            user.PasswordHash);

        if (isNewPasswordSameAsCurrent)
        {
            throw new BadRequestException(
                "New password cannot be the same as the current password");
        }

        user.PasswordHash = PasswordHasher.HashPassword(
            request.Model.NewPassword);

        await staffRepository.UpdateAsync(user, cancellationToken);

        var emailModel = new EmailModel
        {
            Email = user.Email,
            Subject = "Your RetailTech account password updated",
            Message = $"Dear {user.FirstName}, we are letting you know that you have successfully changed the password in your RetailTech profile.  Thank you!"
        };

        await emailSender.SendAsync(emailModel);
    }
}