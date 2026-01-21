using MediatR;
using NettoRetail.Application.Exceptions;
using NettoRetail.Application.Interfaces;
using NettoRetail.Application.Models.Request.Customer;
using NettoRetail.Application.Models.Response;
using NettoRetail.Application.Services.ILinkService;
using NettoRetail.Infrastructure.Interfaces;
using NettoRetail.Shared.Interfaces.Repositories;

namespace NettoRetail.Application.CQRS.Customers.Queries;

public record SendResetPasswordLinkCommand(SendResetPasswordLinkRequest Model) : IRequest<SendResetPasswordLinkResponse>;

public class SendResetPasswordLinkCommandHandler(ICustomerRepository customerRepository,
    ILinkService linkService,
    IEmailBuilderService emailBuilderService,
    IEmailSender emailSender)
    : IRequestHandler<SendResetPasswordLinkCommand, SendResetPasswordLinkResponse>
{
    public async Task<SendResetPasswordLinkResponse> Handle(SendResetPasswordLinkCommand command, CancellationToken cancellationToken)
    {
        var isAccountWithSuchEmailExists = await customerRepository.ExistsActiveCustomerByAccountEmailAsync(
            command.Model.Email,
            cancellationToken);

        if (!isAccountWithSuchEmailExists) throw new NotFoundException("There is no active customer account with this email.");

        var link = linkService.CreateCustomerResetPasswordLink(command.Model.Email);

        var emailModel = await emailBuilderService.BuildCustomerPasswordResetEmailAsync(command.Model.Email, link);
        await emailSender.SendAsync(emailModel);

        var response = new SendResetPasswordLinkResponse();
        response.Message = "The recovery link was sent to your email.";
        response.Link = link;

        return response;
    }
}