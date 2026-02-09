using Mapster;
using MediatR;
using NettoRetail.Application.Exceptions;
using NettoRetail.Application.Interfaces;
using NettoRetail.Application.Models.Request.Customer;
using NettoRetail.Application.Models.Response;
using NettoRetail.Application.Services.ILinkService;
using NettoRetail.DataAccess.Specifications.Account;
using NettoRetail.Domain.Entities;
using NettoRetail.Infrastructure.Interfaces;
using NettoRetail.Shared.Constants;
using NettoRetail.Shared.Interfaces.Repositories;
using NettoRetail.Shared.Interfaces.Utilities;
using NettoRetail.Shared.Utilities;

namespace NettoRetail.Application.CQRS.Customers.Commands;

public record SignUpCustomerCommand(int ShopId, SignUpCustomerRequest Model) : IRequest<SignUpCustomerResponse>;

public class SignUpCustomerCommandHandler(
    IAccountRepository accountRepository,
    ICustomerRepository customerRepository,
    ILinkService linkService,
    IEmailBuilderService emailBuilderService,
    IEmailSender emailSender,
    IUnitOfWork unitOfWork)
    : IRequestHandler<SignUpCustomerCommand, SignUpCustomerResponse>
{
    public async Task<SignUpCustomerResponse> Handle(SignUpCustomerCommand request, CancellationToken cancellationToken)
    {
        var emailSpecification = new GetAccountByEmailSpecification(request.Model.Email);

        if (await accountRepository.Exists(emailSpecification, cancellationToken))
           throw new ConflictException("This email address is already signed up. Please Sign in");

        var account = request.Model.Adapt<AccountEntity>();
        account.RoleId = RoleConstants.Customer;
        account.ShopId = request.ShopId;

        var customer = request.Model.Adapt<CustomerEntity>();

        var initialPassword = request.Model.Password;
        account.PasswordHash = PasswordHasher.HashPassword(initialPassword);

        customer.AccountId = account.Id;

        await SaveEntitiesToDbAsync(account, customer, cancellationToken);

        var link = linkService.CreateCustomerAccountVerificationLink(request.Model.Email);

        var emailModel = await emailBuilderService.BuildCustomerAccountActivationEmailAsync(account.Email, account.FirstName, link);
        await emailSender.SendAsync(emailModel, account.Id);

        var response = account.Adapt<SignUpCustomerResponse>();

        customer.Adapt(response);

        response.ActivationLink = link;

        return response;
    }

    private async Task SaveEntitiesToDbAsync(AccountEntity account, CustomerEntity customer, CancellationToken cancellationToken)
    {
        using var transaction = unitOfWork.BeginTransaction();

        try
        {
            await accountRepository.AddAsync(account, cancellationToken);
            await customerRepository.AddAsync(customer, cancellationToken);
            transaction.Commit();
        }
        catch
        {
            transaction.Rollback();
            throw;
        }
    }
}
