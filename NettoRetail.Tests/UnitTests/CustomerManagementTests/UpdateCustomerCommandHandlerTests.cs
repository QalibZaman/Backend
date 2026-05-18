using System.Data;
using MapsterMapper;
using NettoRetail.Application.CQRS.Customers.Commands;
using NettoRetail.Application.Exceptions;
using NettoRetail.Application.Interfaces;
using NettoRetail.Application.Models.Request.Customer;
using NettoRetail.Application.Services.ICountryValidationService;
using NettoRetail.DataAccess.Specifications.Country;
using NettoRetail.DataAccess.Specifications.Customer;
using NettoRetail.Domain.Entities;
using NettoRetail.Domain.Enums;
using NettoRetail.Shared.Interfaces.Repositories;
using NettoRetail.Shared.Interfaces.Utilities;
using NettoRetail.Tests.Helpers;
using NSubstitute;

namespace NettoRetail.Tests.UnitTests.CustomerManagementTests;

public class UpdateCustomerCommandHandlerTests
{
    private readonly ICountryDetailsValidationService _countryValidator;
    private readonly ICustomerRepository _customerRepository;
    private readonly ICountryRepository _countryRepository;
    private readonly ISessionTerminationService _sessionTerminationService;
    private readonly IUnitOfWork _unitOfWork;
    private readonly IDbTransaction _transaction;
    private readonly IMapper _mapper;
    private readonly UpdateCustomerCommandHandler _handler;

    public UpdateCustomerCommandHandlerTests()
    {
        _countryValidator = Substitute.For<ICountryDetailsValidationService>();
        _customerRepository = Substitute.For<ICustomerRepository>();
        _countryRepository = Substitute.For<ICountryRepository>();
        _sessionTerminationService = Substitute.For<ISessionTerminationService>();
        _unitOfWork = Substitute.For<IUnitOfWork>();
        _transaction = Substitute.For<IDbTransaction>();
        _mapper = MapperFactory.Create();

        _unitOfWork.BeginTransaction().Returns(_transaction);

        _handler = new UpdateCustomerCommandHandler(
            _countryValidator,
            _customerRepository,
            _countryRepository,
            _sessionTerminationService,
            _unitOfWork,
            _mapper);
    }

    [Fact]
    public async Task Handle_ShouldThrowNotFound_WhenCustomerDoesNotExist()
    {
        var command = new UpdateCustomerCommand("fddfdf", CreateRequest());

        _customerRepository.GetAsync(Arg.Any<GetCustomerByLoyaltyIdWithOrdersSpecification>(), Arg.Any<CancellationToken>())
            .Returns((CustomerEntity?)null);
        _countryRepository.GetAsync(Arg.Any<GetCountryByIdSpecification>(), Arg.Any<CancellationToken>())
            .Returns(CreateCountry());

        await Assert.ThrowsAsync<NotFoundException>(() => _handler.Handle(command, CancellationToken.None));

        await _customerRepository.DidNotReceive().UpdateAsync(Arg.Any<CustomerEntity>(), Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task Handle_ShouldThrowNotFound_WhenCountryDoesNotExist()
    {
        var customer = CreateCustomer();
        var command = new UpdateCustomerCommand(customer.LoyaltyId!, CreateRequest());

        _customerRepository.GetAsync(Arg.Any<GetCustomerByLoyaltyIdWithOrdersSpecification>(), Arg.Any<CancellationToken>())
            .Returns(customer);
        _countryRepository.GetAsync(Arg.Any<GetCountryByIdSpecification>(), Arg.Any<CancellationToken>())
            .Returns((CountryEntity?)null);

        await Assert.ThrowsAsync<NotFoundException>(() => _handler.Handle(command, CancellationToken.None));

        await _customerRepository.DidNotReceive().UpdateAsync(Arg.Any<CustomerEntity>(), Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task Handle_ShouldThrowNotFound_WhenEditingDeactivatedCustomerWithoutReactivating()
    {
        var customer = CreateCustomer(status: StaffStatusEnum.Deactivated);
        var command = new UpdateCustomerCommand(customer.LoyaltyId!, CreateRequest(isActive: false));

        _countryRepository.GetAsync(Arg.Any<GetCountryByIdSpecification>(), Arg.Any<CancellationToken>())
            .Returns(CreateCountry());

        await Assert.ThrowsAsync<NotFoundException>(() => _handler.Handle(command, CancellationToken.None));

        await _customerRepository.DidNotReceive().UpdateAsync(Arg.Any<CustomerEntity>(), Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task Handle_ShouldTerminateSessions_WhenEmailChangesForCustomerWithPassword()
    {
        var customer = CreateCustomer();
        var request = CreateRequest(email: "updated@example.com");
        var command = new UpdateCustomerCommand(customer.LoyaltyId!, request);

        _customerRepository.GetAsync(Arg.Any<GetCustomerByLoyaltyIdWithOrdersSpecification>(), Arg.Any<CancellationToken>())
            .Returns(customer);
        _customerRepository.GetAsync(Arg.Any<GetCustomerByEmailSpecification>(), Arg.Any<CancellationToken>())
            .Returns((CustomerEntity?)null);
        _countryRepository.GetAsync(Arg.Any<GetCountryByIdSpecification>(), Arg.Any<CancellationToken>())
            .Returns(CreateCountry());

        await _handler.Handle(command, CancellationToken.None);

        await _sessionTerminationService.Received(1)
            .TerminateSessionsAsync(customer.Id, Arg.Any<CancellationToken>());
        await _customerRepository.Received(1)
            .UpdateAsync(
                Arg.Is<CustomerEntity>(x => x.Email == request.Email && x.Status == StaffStatusEnum.Offline),
                Arg.Any<CancellationToken>());
        _transaction.Received(1).Commit();
    }

    [Fact]
    public async Task Handle_ShouldThrowConflict_WhenAnotherCustomerAlreadyUsesEmail()
    {
        var customer = CreateCustomer();
        var request = CreateRequest(email: "taken@example.com");
        var command = new UpdateCustomerCommand(customer.LoyaltyId!, request);

        _customerRepository.GetAsync(Arg.Any<GetCustomerByLoyaltyIdWithOrdersSpecification>(), Arg.Any<CancellationToken>())
            .Returns(customer);
        _customerRepository.GetAsync(Arg.Any<GetCustomerByEmailSpecification>(), Arg.Any<CancellationToken>())
            .Returns(CreateCustomer(id: Guid.NewGuid(), loyaltyId: "OTHER01", email: request.Email));
        _countryRepository.GetAsync(Arg.Any<GetCountryByIdSpecification>(), Arg.Any<CancellationToken>())
            .Returns(CreateCountry());

        await Assert.ThrowsAsync<ConflictException>(() => _handler.Handle(command, CancellationToken.None));

        await _customerRepository.DidNotReceive().UpdateAsync(Arg.Any<CustomerEntity>(), Arg.Any<CancellationToken>());
        await _sessionTerminationService.DidNotReceive()
            .TerminateSessionsAsync(Arg.Any<Guid>(), Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task Handle_ShouldRollbackTransaction_WhenRepositoryUpdateFails()
    {
        var customer = CreateCustomer();
        var request = CreateRequest();
        var command = new UpdateCustomerCommand(customer.LoyaltyId!, request);

        _customerRepository.GetAsync(Arg.Any<GetCustomerByLoyaltyIdWithOrdersSpecification>(), Arg.Any<CancellationToken>())
            .Returns(customer);
        _customerRepository.GetAsync(Arg.Any<GetCustomerByEmailSpecification>(), Arg.Any<CancellationToken>())
            .Returns((CustomerEntity?)null);
        _countryRepository.GetAsync(Arg.Any<GetCountryByIdSpecification>(), Arg.Any<CancellationToken>())
            .Returns(CreateCountry());
        _customerRepository.UpdateAsync(Arg.Any<CustomerEntity>(), Arg.Any<CancellationToken>())
            .Returns(_ => Task.FromException<CustomerEntity>(new InvalidOperationException("DB failure")));

        await Assert.ThrowsAsync<InvalidOperationException>(() => _handler.Handle(command, CancellationToken.None));

        _transaction.DidNotReceive().Commit();
    }

    [Fact]
    public async Task Handle_ShouldThrowConflict_WhenDeactivatingCustomerWithActiveOrders()
    {
        var customer = CreateCustomer();
        customer.Order =
        [
            new OrderEntity
            {
                Id = 1,
                CreationDate = DateTime.UtcNow,
                Creator = "system",
                OrderStatus = OrderStatusEnum.Processing,
                Delivery = new DeliveryEntity { DeliveryType = OrderDeliveryTypeEnum.Courier },
                CustomerId = customer.Id,
                OrderItems = []
            }
        ];

        var request = CreateRequest();
        request.IsActive = false;
        var command = new UpdateCustomerCommand(customer.LoyaltyId!, request);

        _customerRepository.GetAsync(Arg.Any<GetCustomerByLoyaltyIdWithOrdersSpecification>(), Arg.Any<CancellationToken>())
            .Returns(customer);
        _customerRepository.GetAsync(Arg.Any<GetCustomerByEmailSpecification>(), Arg.Any<CancellationToken>())
            .Returns((CustomerEntity?)null);
        _countryRepository.GetAsync(Arg.Any<GetCountryByIdSpecification>(), Arg.Any<CancellationToken>())
            .Returns(CreateCountry());

        await Assert.ThrowsAsync<ConflictException>(() => _handler.Handle(command, CancellationToken.None));

        await _customerRepository.DidNotReceive().UpdateAsync(Arg.Any<CustomerEntity>(), Arg.Any<CancellationToken>());
        _transaction.DidNotReceive().Commit();
    }

    [Fact]
    public async Task Handle_ShouldDeactivateAndTerminateSessions_WhenDeactivatingCustomerWithNoActiveOrders()
    {
        var customer = CreateCustomer();
        var command = new UpdateCustomerCommand(customer.LoyaltyId!, CreateRequest(isActive: false));

        _customerRepository.GetAsync(Arg.Any<GetCustomerByLoyaltyIdWithOrdersSpecification>(), Arg.Any<CancellationToken>())
            .Returns(customer);
        _customerRepository.GetAsync(Arg.Any<GetCustomerByEmailSpecification>(), Arg.Any<CancellationToken>())
            .Returns((CustomerEntity?)null);
        _countryRepository.GetAsync(Arg.Any<GetCountryByIdSpecification>(), Arg.Any<CancellationToken>())
            .Returns(CreateCountry());

        await _handler.Handle(command, CancellationToken.None);

        await _customerRepository.Received(1)
            .UpdateAsync(
                Arg.Is<CustomerEntity>(x => x.Status == StaffStatusEnum.Deactivated),
                Arg.Any<CancellationToken>());
        await _sessionTerminationService.Received(1)
            .TerminateSessionsAsync(customer.Id, Arg.Any<CancellationToken>());
        _transaction.Received(1).Commit();
    }

    [Fact]
    public async Task Handle_ShouldSetStatusToOffline_WhenReactivatingCustomerWithPassword()
    {
        var customer = CreateCustomer(status: StaffStatusEnum.Deactivated, passwordUpdatedAt: DateTime.UtcNow);
        var command = new UpdateCustomerCommand(customer.LoyaltyId!, CreateRequest(isActive: true));


        _customerRepository.GetAsync(Arg.Any<GetCustomerByLoyaltyIdWithOrdersSpecification>(), Arg.Any<CancellationToken>())
            .Returns(customer);
        _customerRepository.GetAsync(Arg.Any<GetCustomerByEmailSpecification>(), Arg.Any<CancellationToken>())
            .Returns((CustomerEntity?)null);
        _countryRepository.GetAsync(Arg.Any<GetCountryByIdSpecification>(), Arg.Any<CancellationToken>())
            .Returns(CreateCountry());

        await _handler.Handle(command, CancellationToken.None);

        await _customerRepository.Received(1)
            .UpdateAsync(
                Arg.Is<CustomerEntity>(x => x.Status == StaffStatusEnum.Offline),
                Arg.Any<CancellationToken>());
        _transaction.Received(1).Commit();
    }

    private static UpdateCustomerRequest CreateRequest(
        string? email = null,
        bool? isActive = true,
        string? phoneNumber = null)
    {
        return new UpdateCustomerRequest
        {
            FirstName = "John",
            LastName = "Doe",
            Email = email ?? "john@example.com",
            CountryId = 21,
            PhoneNumber = phoneNumber ?? "123 456 789",
            Remarks = "Valid remarks",
            IsActive = isActive
        };
    }
    private static CustomerEntity CreateCustomer(
        Guid? id = null,
        string loyaltyId = "ABC123",
        string email = "john@example.com",
        StaffStatusEnum status = StaffStatusEnum.Online,
        DateTime? passwordUpdatedAt = null,
        List<OrderEntity>? orders = null)
    {
        return new CustomerEntity
        {
            Id = id ?? Guid.NewGuid(),
            LoyaltyId = loyaltyId,
            FirstName = "John",
            LastName = "Doe",
            Email = email,
            PasswordHash = "hash",
            CountryId = 21,
            PhoneNumber = "+48 123 456 789",
            Remarks = "Original remarks",
            Status = status,
            PasswordUpdatedAt = passwordUpdatedAt ?? DateTime.UtcNow,
            Order = orders ?? [],
            RoleId = 1
        };
    }

    private static CountryEntity CreateCountry()
    {
        return new CountryEntity
        {
            Id = 21,
            Name = "Poland",
            PhoneCode = "+48",
            PhoneMask = "^\\d{3} \\d{3} \\d{3}$",
            ZipCodeMask = "^\\d{2}-\\d{3}$"
        };
    }
}
