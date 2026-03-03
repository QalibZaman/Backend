using Mapster;
using MediatR;
using NettoRetail.Application.Exceptions;
using NettoRetail.Application.Interfaces;
using NettoRetail.Application.Models.Request.Customer;
using NettoRetail.Application.Models.Response;
using NettoRetail.Application.Services.ILinkService;
using NettoRetail.DataAccess.Specifications.Customer;
using NettoRetail.Domain.Entities;
using NettoRetail.Infrastructure.Interfaces;
using NettoRetail.Shared.Constants;
using NettoRetail.Shared.Interfaces.Repositories;
using NettoRetail.Shared.Utilities;

namespace NettoRetail.Application.CQRS.Customers.Commands;

public record SignUpCustomerCommand(int ShopId, SignUpCustomerRequest Model) : IRequest<SignUpCustomerResponse>;

public class SignUpCustomerCommandHandler(
    ICustomerRepository customerRepository,
    ILinkService linkService,
    IEmailBuilderService emailBuilderService,
    IEmailSender emailSender)
    : IRequestHandler<SignUpCustomerCommand, SignUpCustomerResponse>
{
    public async Task<SignUpCustomerResponse> Handle(SignUpCustomerCommand request, CancellationToken cancellationToken)
    {
        var emailSpecification = new GetCustomerByEmailAndShopIdSpecification(request.Model.Email, request.ShopId);

        if (await customerRepository.Exists(emailSpecification, cancellationToken))
           throw new ConflictException("This email address is already signed up. Please Sign in");

        var customer = request.Model.Adapt<CustomerEntity>();

        customer.RoleId = RoleConstants.Customer;
        customer.ShopId = request.ShopId;

        var initialPassword = request.Model.Password;
        customer.PasswordHash = PasswordHasher.HashPassword(initialPassword);

        await customerRepository.AddAsync(customer, cancellationToken);

        var link = linkService.CreateCustomerAccountVerificationLink(request.Model.Email, request.ShopId);

        var emailModel = await emailBuilderService.BuildCustomerAccountActivationEmailAsync(customer.Email, customer.FirstName, link);
        await emailSender.SendAsync(emailModel, customer.Id);

        var response = customer.Adapt<SignUpCustomerResponse>();

        customer.Adapt(response);

        response.ActivationLink = link;

        return response;
    }
}
