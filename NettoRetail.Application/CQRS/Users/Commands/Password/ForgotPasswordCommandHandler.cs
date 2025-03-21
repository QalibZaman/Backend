using System.Security.Claims;
using MediatR;
using Microsoft.Extensions.Options;
using NettoRetail.Application.Exceptions;
using NettoRetail.Application.Interfaces;
using NettoRetail.DataAccess.Specifications.Account;
using NettoRetail.Infrastructure.Interfaces;
using NettoRetail.Infrastructure.Models;
using NettoRetail.Infrastructure.Options;
using NettoRetail.Shared.Interfaces.Repositories;
using NettoRetail.Shared.Utilities;

namespace NettoRetail.Application.CQRS.Users.Commands.Password;

public record ForgotPasswordCommand(string Email) : IRequest;

public class ForgotPasswordCommandHandler(
    IAccountRepository accountRepository,
    IJwtManager jwtManager,
    IEmailSender emailSender,
    IEmailTemplateService emailTemplateService,
    IOptions<ResetPasswordOptions> passwordOptions) : IRequestHandler<ForgotPasswordCommand>
{
    public async Task Handle(ForgotPasswordCommand request, CancellationToken cancellationToken)
    {
        var specification = new GetAccountByEmailSpecification(request.Email);

        var user = await accountRepository
            .GetAsync(specification, cancellationToken) ??
            throw new NotFoundException("User with this email was not found.");

        var claims = new List<Claim> { new(ClaimTypes.Email, user.Email) };
        var resetToken = jwtManager.Generate(claims, TimeSpan.FromHours(passwordOptions.Value.TokenExpirationHours));
        var resetLink = string.Format(passwordOptions.Value.ResetLink, resetToken);

        var model = new { reset_link = resetLink, user_name = user.FirstName };

        var emailBody = await emailTemplateService.RenderEmailTemplateAsync(EmailTemplates.ResetPassword, model);

        var emailModel = new EmailModel
        {
            Email = user.Email,
            Subject = "Reset Your Password",
            Message = emailBody
        };

        await emailSender.SendAsync(emailModel);
    }
}
