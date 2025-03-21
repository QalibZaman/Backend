using MediatR;
using NettoRetail.Application.Exceptions;
using NettoRetail.Application.Models.Request.Password;
using NettoRetail.DataAccess.Specifications.Account;
using NettoRetail.Infrastructure.Interfaces;
using NettoRetail.Infrastructure.Models;
using NettoRetail.Shared.Interfaces.Repositories;
using NettoRetail.Shared.Utilities;

namespace NettoRetail.Application.CQRS.Users.Commands.Password;

public record ChangePasswordCommand(Guid UserId, ChangePasswordRequest Model) : IRequest;

public class ChangePasswordCommandHandler(
    IAccountRepository accountRepository,
    IEmailSender emailSender,
    IEmailTemplateService emailTemplateService)
    : IRequestHandler<ChangePasswordCommand>
{
    public async Task Handle(ChangePasswordCommand request, CancellationToken cancellationToken)
    {
        var specification = new GetAccountByIdSpecification(request.UserId);

        var user = await accountRepository
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

        await accountRepository.UpdateAsync(user, cancellationToken);

        var model = new { user_name = user.FirstName };

        var emailBody = await emailTemplateService.RenderEmailTemplateAsync(EmailTemplates.PasswordChange, model);

        var emailModel = new EmailModel
        {
            Email = user.Email,
            Subject = "Your RetailTech account password updated",
            Message = emailBody
        };

        await emailSender.SendAsync(emailModel);
    }
}