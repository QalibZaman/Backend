using MediatR;
using NettoRetail.Application.Exceptions;
using NettoRetail.Application.Interfaces;
using NettoRetail.Application.Models.Request.Customer;
using NettoRetail.Application.Services.ILinkService;
using NettoRetail.DataAccess.Specifications.Account;
using NettoRetail.Infrastructure.Interfaces;
using NettoRetail.Shared.Interfaces.Repositories;

namespace NettoRetail.Application.CQRS.Customers.Commands;

public record SendResetPasswordLinkCommand(SendResetPasswordLinkRequest Request) : IRequest<string>;

public class SendResetPasswordLinkCommandHandler(IAccountRepository accountRepository,
    ILinkService linkService,
    IEmailBuilderService emailBuilderService,
    IEmailSender emailSender)
    : IRequestHandler<SendResetPasswordLinkCommand, string>
{
    public async Task<string> Handle(SendResetPasswordLinkCommand command, CancellationToken cancellationToken)
    {
        var spec = new GetAccountByEmailSpecification(command.Request.Email);
        var isAccountWithSuchEmailExists = await accountRepository.Exists(spec, cancellationToken);

        if (!isAccountWithSuchEmailExists) throw new NotFoundException("An account with this email does not exist.");

        var link = linkService.CreateCustomerResetPasswordLink(command.Request.Email);

        var emailModel = await emailBuilderService.BuildCustomerPasswordResetEmailAsync(command.Request.Email, link);
        await emailSender.SendAsync(emailModel);

        return "The recovery link was sent to your email.";
    }
}