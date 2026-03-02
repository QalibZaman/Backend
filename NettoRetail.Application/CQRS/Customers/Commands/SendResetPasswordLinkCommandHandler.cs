using MediatR;
using NettoRetail.Application.Exceptions;
using NettoRetail.Application.Interfaces;
using NettoRetail.Application.Models.Request.Customer;
using NettoRetail.Application.Models.Response;
using NettoRetail.Application.Services.ILinkService;
using NettoRetail.DataAccess.Specifications.Customer;
using NettoRetail.Infrastructure.Interfaces;
using NettoRetail.Shared.Interfaces.Repositories;

namespace NettoRetail.Application.CQRS.Customers.Queries;

public record SendResetPasswordLinkCommand(SendResetPasswordLinkRequest Model) : IRequest<SendResetPasswordLinkResponse>;

public class SendResetPasswordLinkCommandHandler(
    ICustomerRepository customerRepository,
    ILinkService linkService,
    IEmailBuilderService emailBuilderService,
    IEmailSender emailSender)
    : IRequestHandler<SendResetPasswordLinkCommand, SendResetPasswordLinkResponse>
{
    public async Task<SendResetPasswordLinkResponse> Handle(SendResetPasswordLinkCommand command, CancellationToken cancellationToken)
    {
        var spec = new GetCustomerByEmailAndShopIdSpecification(command.Model.Email, command.Model.ShopId);
        var customer = await customerRepository.GetAsync(spec, cancellationToken)
            ?? throw new BadRequestException("There is no active account with this email within this shop.");

        var link = linkService.CreateCustomerResetPasswordLink(command.Model.Email, command.Model.ShopId);
        var emailModel = await emailBuilderService.BuildCustomerPasswordResetEmailAsync(command.Model.Email, link);
        await emailSender.SendAsync(emailModel, customer.Id);

        var response = new SendResetPasswordLinkResponse
        {
            Message = "The recovery link was sent to your email.",
            Link = link
        };

        return response;
    }
}