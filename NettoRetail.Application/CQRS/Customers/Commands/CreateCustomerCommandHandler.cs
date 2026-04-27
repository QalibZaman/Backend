using MapsterMapper;
using MediatR;
using NettoRetail.Application.Exceptions;
using NettoRetail.Application.Models.Country;
using NettoRetail.Application.Models.Request.Customer;
using NettoRetail.Application.Models.Response;
using NettoRetail.Application.Services.ICountryValidationService;
using NettoRetail.Application.Utilities;
using NettoRetail.DataAccess.Specifications.Country;
using NettoRetail.DataAccess.Specifications.Customer;
using NettoRetail.DataAccess.Specifications.Role;
using NettoRetail.Domain.Entities;
using NettoRetail.Shared.Constants;
using NettoRetail.Shared.Interfaces.Repositories;
using NettoRetail.Shared.Interfaces.Utilities;
using NettoRetail.Shared.Utilities;

namespace NettoRetail.Application.CQRS.Customers.Commands;

public record CreateCustomerCommand(int ShopId, CreateCustomerRequest Model) : IRequest<CreateCustomerResponse>;

public class CreateCustomerCommandHandler(
    ICountryDetailsValidationService countryValidator,
    ICustomerRepository customerRepository,
    ICountryRepository countryRepository,
    IRoleRepository roleRepository,
    IUnitOfWork unitOfWork,
    IMapper mapper)
    : IRequestHandler<CreateCustomerCommand, CreateCustomerResponse>
{
    public async Task<CreateCustomerResponse> Handle(CreateCustomerCommand request, CancellationToken cancellationToken)
    {
        CountryEntity? country = null;

        if (!string.IsNullOrEmpty(request.Model.PhoneNumber))
        {
            var countryValidationModel = new CountryValidationModel
            {
                CountryId = request.Model.CountryId,
                PhoneNumber = request.Model.PhoneNumber!
            };

            var countryValidationOptions = new CountryValidationOptions
            {
                CheckForPhoneNumber = true,
                CheckForZipCode = false
            };

            await countryValidator.ValidateAsync(countryValidationModel, cancellationToken, countryValidationOptions);

            var countrySpecification = new GetCountryByIdSpecification(request.Model.CountryId);
            country = await countryRepository.GetAsync(countrySpecification, cancellationToken);
        }

        var emailSpecification = new GetCustomerByEmailSpecification(request.Model.Email);
        var account = await customerRepository.Exists(emailSpecification, cancellationToken);

        if (account)
        {
            throw new ConflictException("Account with this email already exists");
        }

        var roleSpecification = new GetRoleByIdSpecification(RoleConstants.Customer);
        var role = await roleRepository.GetAsync(roleSpecification, cancellationToken);

        var customerEntity = mapper.Map<CustomerEntity>(request.Model);

        customerEntity.RoleId = role!.Id;
        customerEntity.ShopId = request.ShopId;

        var initialPassword = Guid.NewGuid().ToString();
        customerEntity.PasswordHash = PasswordHasher.HashPassword(initialPassword);

        customerEntity.LoyaltyId = await GenerateUniqueLoyaltyIdAsync(cancellationToken);
        customerEntity.IsActive = true;

        if (request.Model.PhoneNumber != null)
        {
            customerEntity.PhoneNumber = $"{country!.PhoneCode} {request.Model.PhoneNumber}";
        }

        using var transaction = unitOfWork.BeginTransaction();

        await customerRepository.AddAsync(customerEntity, cancellationToken);
        transaction.Commit();

        var response = mapper.Map<CreateCustomerResponse>(customerEntity);
        response.Country = country == null ? null : mapper.Map<CountryResponse>(country);

        return response;
    }

    private async Task<string> GenerateUniqueLoyaltyIdAsync(CancellationToken cancellationToken)
    {
        var loyaltyId = UserUtility.GenerateLoyaltyId();

        var specification = new GetCustomerByLoyaltyIdSpecification(loyaltyId);
        var exists = await customerRepository.Exists(specification, cancellationToken);

        if (exists)
        {
            throw new ConflictException("Generated Loyalty ID already exists.");
        }

        return loyaltyId;
    }
}