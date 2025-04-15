using Mapster;
using MediatR;
using NettoRetail.Application.Exceptions;
using NettoRetail.Application.Interfaces;
using NettoRetail.Application.Models.Country;
using NettoRetail.Application.Models.Request.Customer;
using NettoRetail.Application.Services.ICountryValidationService;
using NettoRetail.DataAccess.Specifications.Account;
using NettoRetail.DataAccess.Specifications.Country;
using NettoRetail.DataAccess.Specifications.Customer;
using NettoRetail.Domain.Enums;
using NettoRetail.Shared.Interfaces.Repositories;
using NettoRetail.Shared.Interfaces.Utilities;

namespace NettoRetail.Application.CQRS.Customers.Commands;

public record UpdateCustomerCommand(string LoyaltyId, UpdateCustomerRequest Model) : IRequest;

public class UpdateCustomerCommandHandler(
    IAccountRepository accountRepository,
    ICountryDetailsValidationService countryValidator,
    ICustomerRepository customerRepository,
    ICountryRepository countryRepository,
    ISessionTerminationService sessionTerminationService,
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

            var countryValidationOptions = new CountryValidationOptions()
            {
                CheckForPhoneNumber = true,
                CheckForZipCode = false
            };

            await countryValidator.ValidateAsync(countryValidationModel, cancellationToken, countryValidationOptions);
        }

        var customerLoyaltyIdSpecification = new GetCustomerByLoyaltyIdSpecification(request.LoyaltyId);
        var customer = await customerRepository.GetWithRelatedDataAsync(customerLoyaltyIdSpecification, cancellationToken)
            ?? throw new NotFoundException("There is no customer with such loyalty id.");

        var accountIdSpecification = new GetAccountByIdSpecification(customer.AccountId);
        var account = await accountRepository.GetAsync(accountIdSpecification, cancellationToken)
            ?? throw new NotFoundException("There is no account record for the customer with such loyalty id.");

        var countrySpecification = new GetCountryByIdSpecification(request.Model.CountryId);
        var country = await countryRepository.GetAsync(countrySpecification, cancellationToken);

        request.Model.Adapt(account);
        request.Model.Adapt(customer);

        if (request.Model.PhoneNumber != null)
        {
            customer.PhoneNumber = $"{country!.PhoneCode} {request.Model.PhoneNumber}";
        }

        // Begin transaction to ensure atomicity between status changes, session termination, and persistence.
        // This prevents partial updates in case of failure — for example, if the account status is updated
        // but the sessions aren't terminated, or if only one of the entities is saved.
        var transaction = unitOfWork.BeginTransaction();

        try
        {
            if (account.Status == StaffStatusEnum.Deactivated &&
                (!request.Model.IsActive.HasValue || request.Model.IsActive.Value == false))
            {
                throw new BadRequestException("You cannot edit a deactivated customer unless you are reactivating them.");
            }

            if (request.Model.Email != null && account.Email != request.Model.Email && account.PasswordUpdatedAt != null)
            {
                var emailConflictSpec = new GetAccountByEmailSpecification(request.Model.Email);
                var existingWithEmail = await accountRepository.GetAsync(emailConflictSpec, cancellationToken);
                if (existingWithEmail != null)
                    throw new ConflictException("Another account with this email already exists.");

                // Terminate sessions when email is changed
                await sessionTerminationService.TerminateSessionsAsync(account.Id, cancellationToken);

                account.Status = StaffStatusEnum.Offline;
            }

            if (request.Model.IsActive.HasValue)
            {
                if (request.Model.IsActive.Value && account.Status == StaffStatusEnum.Deactivated)
                {
                    account.Status = account.PasswordUpdatedAt == null
                        ? StaffStatusEnum.CreatePassword
                        : StaffStatusEnum.Offline;
                }
                else if (!request.Model.IsActive.Value && account.Status != StaffStatusEnum.Deactivated)
                {
                    var activeStatuses = new[]
                    {
                        OrderStatusEnum.New,
                        OrderStatusEnum.Processing,
                        OrderStatusEnum.InProgress
                    };

                    var hasActiveOrders = customer.Order?.Any(order => activeStatuses.Contains(order.OrderStatus)) == true;

                    if (hasActiveOrders)
                    {
                        throw new ConflictException("This customer has active orders. Please complete or cancel them before deactivating.");
                    }

                    account.Status = StaffStatusEnum.Deactivated;
                    await sessionTerminationService.TerminateSessionsAsync(account.Id, cancellationToken);
                }
            }

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
