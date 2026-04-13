using MediatR;
using NettoRetail.Application.Exceptions;
using NettoRetail.Application.Interfaces;
using NettoRetail.DataAccess.Specifications.Staff;
using NettoRetail.Infrastructure.Interfaces;
using NettoRetail.Shared.Interfaces.Repositories;

namespace NettoRetail.Application.CQRS.Users.Commands.Password;

public record SendPasswordResetLinkCommand(string Email) : IRequest;

public class SendPasswordResetLinkCommandHandler(
    IStaffRepository accountRepo,
    IEmailBuilderService emailBuilder,
    IEmailSender emailSender
) : IRequestHandler<SendPasswordResetLinkCommand>
{
    public async Task Handle(
        SendPasswordResetLinkCommand request,
        CancellationToken cancellationToken)
    {
        var email = request.Email.Trim().ToLowerInvariant();
        var account = await accountRepo.GetAsync(
            new GetStaffByEmailSpecification(email),
            cancellationToken);

        if (account is null)
            throw new BadRequestException("No account with that email.");

        var emailModel = await emailBuilder
            .BuildResetPasswordEmailAsync(account.Email, account.FirstName);

        await emailSender.SendAsync(emailModel, account.Id);
    }

}
