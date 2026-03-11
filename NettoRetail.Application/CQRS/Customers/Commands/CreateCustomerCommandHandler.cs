using MapsterMapper;
using MediatR;
using NettoRetail.Application.Exceptions;
using NettoRetail.Application.Models.Country;
using NettoRetail.Application.Models.Request.Customer;
using NettoRetail.Application.Models.Response;
using NettoRetail.Application.Services.ICountryValidationService;
using NettoRetail.Application.Utilities;
using NettoRetail.DataAccess.Specifications.Account;
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
        if (!string.IsNullOrEmpty(request.Model.PhoneNumber))
        {
            var countryValidationModel = new CountryValidationModel()
            {
                CountryId = request.Model.CountryId,
                PhoneNumber = request.Model.PhoneNumber!
            };

            var countryValidationOptions = new CountryValidationOptions() { CheckForPhoneNumber = true, CheckForZipCode = false };

            await countryValidator.ValidateAsync(countryValidationModel, cancellationToken, countryValidationOptions);
        }

        var emailSpecification = new GetCustomerByEmailSpecification(request.Model.Email);
        var account = await customerRepository.Exists(emailSpecification, cancellationToken);
        if (account)
            throw new ConflictException("Account with this email already exists");

        var roleSpecification = new GetRoleByIdSpecification(RoleConstants.Customer);
        var role = await roleRepository.GetAsync(roleSpecification, cancellationToken);

        var customerEntity = mapper.Map<CustomerEntity>(request.Model);

        customerEntity.RoleId = role!.Id;
        customerEntity.ShopId = request.ShopId;

        // Every customer is registered with a random password - in future, they'll be able to reset and set theirs when they'll be registered
        // Right now, customers shouldn't be able to log in - and in the future, when they get the activation link, they'll be able to reset their password
        var initialPassword = Guid.NewGuid().ToString();
        customerEntity.PasswordHash = PasswordHasher.HashPassword(initialPassword);

        customerEntity.LoyaltyId = UserUtility.GenerateLoyaltyId();
        customerEntity.IsActive = true;

        var countrySpecification = new GetCountryByIdSpecification(request.Model.CountryId);
        var country = await countryRepository.GetAsync(countrySpecification, cancellationToken);

        if (request.Model.PhoneNumber != null)
        {
            customerEntity.PhoneNumber = $"{country!.PhoneCode} {request.Model.PhoneNumber}";
        }

        var transaction = unitOfWork.BeginTransaction();

        try
        {
            await customerRepository.AddAsync(customerEntity, cancellationToken);
            transaction.Commit();
        }
        catch
        {
            transaction.Rollback();
            throw;
        }

        var response = mapper.Map<CreateCustomerResponse>(customerEntity);
        response.Country = country == null ? null : mapper.Map<CountryResponse>(country);

        return response;
    }
}
