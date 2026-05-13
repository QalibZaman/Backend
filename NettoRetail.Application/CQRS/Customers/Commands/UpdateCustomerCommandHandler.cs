using MapsterMapper;
using MediatR;
using NettoRetail.Application.Exceptions;
using NettoRetail.Application.Interfaces;
using NettoRetail.Application.Models.Country;
using NettoRetail.Application.Models.Request.Customer;
using NettoRetail.Application.Services.ICountryValidationService;
using NettoRetail.DataAccess.Specifications.Country;
using NettoRetail.DataAccess.Specifications.Customer;
using NettoRetail.Domain.Entities;
using NettoRetail.Domain.Enums;
using NettoRetail.Shared.Constants;
using NettoRetail.Shared.Interfaces.Repositories;
using NettoRetail.Shared.Interfaces.Utilities;
using Stripe;

namespace NettoRetail.Application.CQRS.Customers.Commands;

public record UpdateCustomerCommand(string LoyaltyId, UpdateCustomerRequest Model) : IRequest;

public class UpdateCustomerCommandHandler(
    ICountryDetailsValidationService countryValidator,
    ICustomerRepository customerRepository,
    ICountryRepository countryRepository,
    ISessionTerminationService sessionTerminationService,
    IUnitOfWork unitOfWork,
    IMapper mapper) : IRequestHandler<UpdateCustomerCommand>
{
    public async Task Handle(UpdateCustomerCommand request, CancellationToken cancellationToken)
    {
        var customer = await GetCustomerOrThrowAsync(request.LoyaltyId, cancellationToken);
        var country = await GetCountryOrThrowAsync(request.Model.CountryId, cancellationToken);

        await CheckPhoneNumberAsync(request.Model.PhoneNumber, request.Model.CountryId, cancellationToken);
        ValidateCustomerEditable(customer, request.Model);

        var emailChanged = HasEmailChanged(customer.Email, request.Model.Email);
        if (emailChanged)
            await EnsureEmailIsUniqueAsync(customer.Id, request.Model.Email, cancellationToken);

        using var transaction = unitOfWork.BeginTransaction();

        mapper.Map(request.Model, customer);

        if (request.Model.PhoneNumber != null)
            customer.PhoneNumber = $"{country.PhoneCode} {request.Model.PhoneNumber}";

        if (emailChanged && customer.PasswordUpdatedAt != default)
        {
            await sessionTerminationService.TerminateSessionsAsync(customer.Id, cancellationToken);
            customer.Status = StaffStatusEnum.Offline;
        }

        if (request.Model.IsActive.HasValue)
        {
            await ApplyActiveStatusChangeAsync(customer, request.Model.IsActive.Value, cancellationToken);
        }

        await customerRepository.UpdateAsync(customer, cancellationToken);
        transaction.Commit();
    }

    private async Task<CustomerEntity> GetCustomerOrThrowAsync(string loyaltyId, CancellationToken cancellationToken)
    {
        var spec = new GetCustomerByLoyaltyIdWithOrdersSpecification(loyaltyId);
        return await customerRepository.GetAsync(spec, cancellationToken)
            ?? throw new NotFoundException(ValidationErrorMessages.Customer.CustomerNotFoundErrorMessage);
    }

    private async Task<CountryEntity> GetCountryOrThrowAsync(int countryId, CancellationToken cancellationToken)
    {
        var specification = new GetCountryByIdSpecification(countryId);
        return await countryRepository.GetAsync(specification, cancellationToken)
            ?? throw new NotFoundException(ValidationErrorMessages.Country.CountryNotFoundErrorMessage);
    }

    private async Task CheckPhoneNumberAsync(string? phoneNumber, int countryId, CancellationToken cancellationToken)
    {
        if (string.IsNullOrEmpty(phoneNumber))
            return;

        var countryValidationModel = new CountryValidationModel
        {
            CountryId = countryId,
            PhoneNumber = phoneNumber,
        };

        var countryValidationOptions = new CountryValidationOptions
        {
            CheckForPhoneNumber = true,
        };

        await countryValidator.ValidateAsync(countryValidationModel, cancellationToken, countryValidationOptions);
    }

    private static void ValidateCustomerEditable(CustomerEntity customer, UpdateCustomerRequest model)
    {
        if (customer.Status == StaffStatusEnum.Deactivated &&
            (!model.IsActive.HasValue || model.IsActive.Value == false))
        {
            throw new BadRequestException(ValidationErrorMessages.Customer.CannotEditDeactivatedCustomerErrorMessage);
        }
    }

    private static bool HasEmailChanged(string current, string updated)
    {
        return !string.Equals(current, updated, StringComparison.OrdinalIgnoreCase);
    }

    private async Task EnsureEmailIsUniqueAsync(Guid customerId, string email, CancellationToken cancellationToken)
    {
        var specification = new GetCustomerByEmailSpecification(email);
        var existing = await customerRepository.GetAsync(specification, cancellationToken);
        if (existing is not null && existing.Id != customerId)
            throw new ConflictException(ValidationErrorMessages.Common.EmailAlreadyExistsErrorMessage);
    }

    private async Task ApplyActiveStatusChangeAsync(CustomerEntity customer, bool isActive, CancellationToken cancellationToken)
    {
        if (isActive && customer.Status == StaffStatusEnum.Deactivated)
        {
            customer.Status = customer.PasswordUpdatedAt == default
                ? StaffStatusEnum.CreatePassword
                : StaffStatusEnum.Offline;

            return;
        }

        if (!isActive && customer.Status != StaffStatusEnum.Deactivated)
        {
            EnsureNoActionOrders(customer);
            customer.Status = StaffStatusEnum.Deactivated;
            await sessionTerminationService.TerminateSessionsAsync(customer.Id, cancellationToken);
        }
    }

    private static void EnsureNoActionOrders(CustomerEntity customer)
    {
        var activeStatuses = new[]
        {
            OrderStatusEnum.New,
            OrderStatusEnum.Processing,
            OrderStatusEnum.InProgress
        };

        var hasActiveOrders = customer.Order?.Any(x => activeStatuses.Contains(x.OrderStatus)) == true;
        if (hasActiveOrders)
            throw new ConflictException(ValidationErrorMessages.Customer.CustomerHasOrdersErrorMessage);
    }
}
