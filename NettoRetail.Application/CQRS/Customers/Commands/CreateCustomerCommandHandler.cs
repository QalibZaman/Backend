using Mapster;
using MediatR;
using NettoRetail.Application.Exceptions;
using NettoRetail.Application.Interfaces;
using NettoRetail.Application.Models.Request.Customer;
using NettoRetail.Application.Models.Response;
using NettoRetail.Application.Services.ILinkService;
using NettoRetail.DataAccess.Specifications.Account;
using NettoRetail.DataAccess.Specifications.Role;
using NettoRetail.Domain.Entities;
using NettoRetail.Infrastructure.Interfaces;
using NettoRetail.Shared.Interfaces.Repositories;
using NettoRetail.Shared.Interfaces.Utilities;
using NettoRetail.Shared.Utilities;

namespace NettoRetail.Application.CQRS.Customers.Commands;

public record CreateCustomerCommand(CreateCustomerRequest Model) : IRequest<CreateCustomerResponse>;

public class CreateCustomerCommandHandler(
    IAccountRepository accountRepository,
    ICustomerRepository customerRepository,
    ILinkService linkService,
    IRoleRepository roleRepository,
    IEmailBuilderService emailBuilderService,
    IEmailSender emailSender,
    IUnitOfWork unitOfWork)
    : IRequestHandler<CreateCustomerCommand, CreateCustomerResponse>
{
    public async Task<CreateCustomerResponse> Handle(CreateCustomerCommand request, CancellationToken cancellationToken)
    {
        var emailSpecification = new GetAccountByEmailSpecification(request.Model.Email);
        var account = await accountRepository.Exists(emailSpecification, cancellationToken);

        if (account)
           throw new ConflictException("This email address is already signed up. Please Sign in");

        var roleSpecification = new GetRoleByNameSpecification("Customer");
        var role = await roleRepository.GetAsync(roleSpecification, cancellationToken);

        var accountEntity = request.Model.Adapt<AccountEntity>();
        accountEntity.RoleId = role.Id;

        var customerEntity = request.Model.Adapt<CustomerEntity>();

        var initialPassword = request.Model.Password;
        accountEntity.PasswordHash = PasswordHasher.HashPassword(initialPassword);

        customerEntity.AccountId = accountEntity.Id;

        using var transaction = unitOfWork.BeginTransaction();

        try
        {
            await accountRepository.AddAsync(accountEntity, cancellationToken);
            await customerRepository.AddAsync(customerEntity, cancellationToken);
            transaction.Commit();
        }
        catch
        {
            transaction.Rollback();
            throw;
        }

        var link = await linkService.CreateCustomerAccountVerificationLink(request.Model.Email);

        var emailModel = await emailBuilderService.BuildCustomerAccountActivationEmailAsync(accountEntity.Email, accountEntity.FirstName, link);
        await emailSender.SendAsync(emailModel);

        var response = accountEntity.Adapt<CreateCustomerResponse>();

        customerEntity.Adapt(response);

        response.ActivationLink = link;

        return response;
    }
}
