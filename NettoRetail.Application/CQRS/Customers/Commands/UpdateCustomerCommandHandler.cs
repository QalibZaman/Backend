using Mapster;
using MediatR;
using NettoRetail.Application.Exceptions;
using NettoRetail.Application.Models.Country;
using NettoRetail.Application.Models.Request.Customer;
using NettoRetail.Application.Services.ICountryValidationService;
using NettoRetail.DataAccess.Specifications.Account;
using NettoRetail.DataAccess.Specifications.Country;
using NettoRetail.DataAccess.Specifications.Customer;
using NettoRetail.Shared.Interfaces.Repositories;
using NettoRetail.Shared.Interfaces.Utilities;

namespace NettoRetail.Application.CQRS.Customers.Commands;

public record UpdateCustomerCommand(string LoyaltyId, UpdateCustomerRequest Model) : IRequest;

public class UpdateCustomerCommandHandler(
    IAccountRepository accountRepository,
    ICountryDetailsValidationService countryValidator,
    ICustomerRepository customerRepository,
    ICountryRepository countryRepository,
    IUnitOfWork unitOfWork) : IRequestHandler<UpdateCustomerCommand>
{
    public async Task Handle(UpdateCustomerCommand request, CancellationToken cancellationToken)
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

        var customerLoyaltyIdSpecification = new GetCustomerByLoyaltyIdSpecification(request.LoyaltyId);
        var customer = await customerRepository.GetAsync(customerLoyaltyIdSpecification, cancellationToken) ?? throw new NotFoundException("There is no customer with such loyalty id.");

        var accountIdSpecification = new GetAccountByIdSpecification(customer.AccountId);
        var account = await accountRepository.GetAsync(accountIdSpecification, cancellationToken) ?? throw new NotFoundException("There is no account record for the customer with such loyalty id.");

        request.Model.Adapt(account);
        request.Model.Adapt(customer);

        var countrySpecification = new GetCountryByIdSpecification(request.Model.CountryId);
        var country = await countryRepository.GetAsync(countrySpecification, cancellationToken);

        if (request.Model.PhoneNumber != null)
        {
            customer.PhoneNumber = $"{country!.PhoneCode} {request.Model.PhoneNumber}";
        }

        var transaction = unitOfWork.BeginTransaction();

        try
        {
            await accountRepository.UpdateAsync(account, cancellationToken);
            await customerRepository.UpdateAsync(customer, cancellationToken);
            transaction.Commit();
        }
        catch
        {
            transaction.Rollback();
            throw;
        }
    }
}
