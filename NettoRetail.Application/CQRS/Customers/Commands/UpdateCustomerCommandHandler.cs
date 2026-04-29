using MapsterMapper;
using MediatR;
using NettoRetail.Application.Exceptions;
using NettoRetail.Application.Interfaces;
using NettoRetail.Application.Models.Country;
using NettoRetail.Application.Models.Request.Customer;
using NettoRetail.Application.Services.ICountryValidationService;
using NettoRetail.DataAccess.Specifications.Country;
using NettoRetail.DataAccess.Specifications.Customer;
using NettoRetail.Domain.Enums;
using NettoRetail.Shared.Interfaces.Repositories;
using NettoRetail.Shared.Interfaces.Utilities;

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
        var customerLoyaltyIdSpecification = new GetCustomerByLoyaltyIdWithOrdersSpecification(request.LoyaltyId);
        var customer = await customerRepository.GetAsync(customerLoyaltyIdSpecification, cancellationToken)
            ?? throw new NotFoundException("There is no customer with such loyalty id.");

        var countrySpecification = new GetCountryByIdSpecification(request.Model.CountryId);
        var country = await countryRepository.GetAsync(countrySpecification, cancellationToken)
            ?? throw new NotFoundException("There is no country with such id.");

        await CheckPhoneNumberAsync(request.Model.PhoneNumber, request.Model.CountryId, cancellationToken);

        if (customer.Status == StaffStatusEnum.Deactivated &&
            (!request.Model.IsActive.HasValue || request.Model.IsActive.Value == false))
        {
            throw new BadRequestException("You cannot edit a deactivated customer unless you are reactivating them.");
        }

        var emailChanged = !string.Equals(customer.Email, request.Model.Email, StringComparison.OrdinalIgnoreCase);
        if (emailChanged)
        {
            var emailSpecification = new GetCustomerByEmailSpecification(request.Model.Email);
            var customerWithSameEmail = await customerRepository.GetAsync(emailSpecification, cancellationToken);
            if (customerWithSameEmail is not null && customerWithSameEmail.Id != customer.Id)
            {
                throw new ConflictException("Another account with this email already exists.");
            }
        }

        using var transaction = unitOfWork.BeginTransaction();
        mapper.Map(request.Model, customer);

        if (request.Model.PhoneNumber != null)
        {
                customer.PhoneNumber = $"{country.PhoneCode} {request.Model.PhoneNumber}";
        }

        if (emailChanged && customer.PasswordUpdatedAt != default)
        {
                await sessionTerminationService.TerminateSessionsAsync(customer.Id, cancellationToken);
                customer.Status = StaffStatusEnum.Offline;
        }

        if (request.Model.IsActive.HasValue)
        {
                if (request.Model.IsActive.Value && customer.Status == StaffStatusEnum.Deactivated)
                {
                    customer.Status = customer.PasswordUpdatedAt == default
                        ? StaffStatusEnum.CreatePassword
                        : StaffStatusEnum.Offline;
                }
                else if (!request.Model.IsActive.Value && customer.Status != StaffStatusEnum.Deactivated)
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

                    customer.Status = StaffStatusEnum.Deactivated;
                    await sessionTerminationService.TerminateSessionsAsync(customer.Id, cancellationToken);
                }
        }

        await customerRepository.UpdateAsync(customer, cancellationToken);

        transaction.Commit();
        
    }

    private async Task CheckPhoneNumberAsync(string? phoneNumber, int countryId, CancellationToken cancellationToken)
    {
        if (!string.IsNullOrEmpty(phoneNumber))
        {
            var countryValidationModel = new CountryValidationModel
            {
                CountryId = countryId,
                PhoneNumber = phoneNumber
            };

            var countryValidationOptions = new CountryValidationOptions
            {
                CheckForPhoneNumber = true,
                CheckForZipCode = false
            };

            await countryValidator.ValidateAsync(countryValidationModel, cancellationToken, countryValidationOptions);
        }
    }
}
