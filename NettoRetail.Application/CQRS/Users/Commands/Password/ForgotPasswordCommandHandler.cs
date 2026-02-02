using MediatR;
using NettoRetail.Application.Exceptions;
using NettoRetail.Application.Interfaces;
using NettoRetail.DataAccess.Specifications.Account;
using NettoRetail.Infrastructure.Interfaces;
using NettoRetail.Shared.Interfaces.Repositories;

namespace NettoRetail.Application.CQRS.Users.Commands.Password;

public record ForgotPasswordCommand(string Email) : IRequest;

public class ForgotPasswordCommandHandler(
    IAccountRepository accountRepository,
    IEmailSender emailSender,
    IEmailBuilderService emailBuilderService) : IRequestHandler<ForgotPasswordCommand>
{
    public async Task Handle(ForgotPasswordCommand request, CancellationToken cancellationToken)
    {
        var specification = new GetAccountByEmailSpecification(request.Email);

        var user = await accountRepository
            .GetAsync(specification, cancellationToken) ??
            throw new NotFoundException("User with this email was not found.");

        var emailModel = await emailBuilderService.BuildResetPasswordEmailAsync(user.Email, user.FirstName);

        await emailSender.SendAsync(emailModel, user.Id);
    }
}
