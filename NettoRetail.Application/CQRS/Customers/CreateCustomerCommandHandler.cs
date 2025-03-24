using Mapster;
using MediatR;
using NettoRetail.Application.Exceptions;
using NettoRetail.Application.Models.Request.Customer;
using NettoRetail.Application.Models.Response;
using NettoRetail.Application.Services.ICountryValidationService;
using NettoRetail.Application.Utilities;
using NettoRetail.DataAccess.Specifications.Account;
using NettoRetail.DataAccess.Specifications.Country;
using NettoRetail.DataAccess.Specifications.Role;
using NettoRetail.Domain.Entities;
using NettoRetail.Domain.Enums;
using NettoRetail.Shared.Interfaces.Repositories;
using NettoRetail.Shared.Interfaces.Utilities;
using NettoRetail.Shared.Utilities;

namespace NettoRetail.Application.CQRS.Customers;

public record CreateCustomerCommand(CreateCustomerRequest Model) : IRequest<CreateCustomerResponse>;

public class CreateCustomerCommandHandler(
    IAccountRepository accountRepository,
    ICountryDetailsValidationService countryValidator,
    ICountryRepository countryRepository,
    ICustomerRepository customerRepository,
    IRoleRepository roleRepository,
    IUnitOfWork unitOfWork)
    : IRequestHandler<CreateCustomerCommand, CreateCustomerResponse>
{
    public async Task<CreateCustomerResponse> Handle(CreateCustomerCommand request, CancellationToken cancellationToken)
    {
        CountryEntity? country = null;
        if (request.Model.PhoneNumber != null)
        {
            var countrySpecification = new GetCountryByIdSpecification(request.Model.CountryId);
            country = await countryRepository.GetAsync(countrySpecification, cancellationToken);

            if (country == null)
            {
                throw new BadRequestException("Invalid country id");
            }

            var isValid = countryValidator.IsPhoneNumberValid(country, request.Model.PhoneNumber);

            if (!isValid)
            {
                throw new BadRequestException("Invalid phone number for the given country");
            }
        }

        var emailSpecification = new GetAccountByEmailSpecification(request.Model.Email);
        var account = await accountRepository.GetAsync(emailSpecification, cancellationToken);

        if (account != null)
        {
            throw new ConflictException("Account with this email already exists");
        }

        var roleSpecification = new GetRoleByNameSpecification(RoleEnum.Customer);
        var role = await roleRepository.GetAsync(roleSpecification, cancellationToken);

        var accountEntity = request.Model.Adapt<AccountEntity>();
        var customerEntity = request.Model.Adapt<CustomerEntity>();

        accountEntity.RoleId = role!.Id;
        accountEntity.PasswordHash = PasswordHasher.HashPassword(request.Model.Password);

        customerEntity.AccountId = accountEntity.Id;
        customerEntity.LoyaltyId = UserUtility.GenerateLoyaltyId();

        if (request.Model.PhoneNumber != null)
        {
            customerEntity.PhoneNumber = $"{country!.PhoneCode} {request.Model.PhoneNumber}";
        }

        var transaction = unitOfWork.BeginTransaction();

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

        var response = accountEntity.Adapt<CreateCustomerResponse>();

        customerEntity.Adapt(response);
        response.Country = country?.Adapt<CountryResponse>() ?? null;

        return response;
    }
}
