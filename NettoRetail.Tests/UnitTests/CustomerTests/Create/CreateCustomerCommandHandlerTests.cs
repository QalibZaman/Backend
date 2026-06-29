using System.Data;
using MapsterMapper;
using Microsoft.EntityFrameworkCore.Storage;
using NettoRetail.Application.CQRS.Customers.Commands;
using NettoRetail.Application.Exceptions;
using NettoRetail.Application.Models.Country;
using NettoRetail.Application.Models.Request.Customer;
using NettoRetail.Application.Models.Response;
using NettoRetail.Application.Services.ICountryValidationService;
using NettoRetail.DataAccess.Specifications.Country;
using NettoRetail.DataAccess.Specifications.Customer;
using NettoRetail.DataAccess.Specifications.Role;
using NettoRetail.Domain.Entities;
using NettoRetail.Shared.Constants;
using NettoRetail.Shared.Interfaces.Repositories;
using NettoRetail.Shared.Interfaces.Utilities;
using NSubstitute;
using Xunit.Abstractions;

namespace NettoRetail.Tests.UnitTests.CustomerTests;

public class CreateCustomerCommandHandlerTests
{
    private readonly ICountryDetailsValidationService _countryValidator;
    private readonly ICustomerRepository _customerRepository;
    private readonly ICountryRepository _countryRepository;
    private readonly IRoleRepository _roleRepository;
    private readonly IUnitOfWork _unitOfWork;
    private readonly IMapper _mapper;
    private readonly CreateCustomerCommandHandler _handler;
    private readonly IDbTransaction _transaction;

    public CreateCustomerCommandHandlerTests()
    {
        _countryValidator =  Substitute.For<ICountryDetailsValidationService>();
        _customerRepository = Substitute.For<ICustomerRepository>();
        _countryRepository = Substitute.For<ICountryRepository>();
        _roleRepository = Substitute.For<IRoleRepository>();
        _unitOfWork = Substitute.For<IUnitOfWork>();
        _mapper = Substitute.For<IMapper>();
        _transaction = Substitute.For<IDbTransaction>();
        
        _unitOfWork.BeginTransaction().Returns(_transaction);
        
        _handler = new CreateCustomerCommandHandler(
            _countryValidator,
            _customerRepository,
            _countryRepository,
            _roleRepository,
            _unitOfWork,
            _mapper);
        
    }
    
    [Fact]
    public async Task Handle_WhenEmailAlreadyExists_ThrowsConflictException()
    {
        var request = CreateValidCustomerRequest();
        var command = new CreateCustomerCommand(1, request);

        _customerRepository
            .ExistsAsync(Arg.Any<GetCustomerByEmailSpecification>(), Arg.Any<CancellationToken>())
            .Returns(true);

        var exception = await Assert.ThrowsAsync<ConflictException>(() =>
            _handler.Handle(command, CancellationToken.None));

        Assert.Equal(HandlerConstants.Customer.EmailIsAlreadyUsedMessage, exception.Message);

        await _customerRepository.DidNotReceive()
            .AddAsync(Arg.Any<CustomerEntity>(), Arg.Any<CancellationToken>());

        _unitOfWork.DidNotReceive().BeginTransaction();
    }

    [Fact]
    public async Task Handle_WhenGeneratedIdAlreadyExists_ThrowsConflictException()
    {
        var request = CreateValidCustomerRequest();
        var command = new CreateCustomerCommand(1, request);
        var role = new RoleEntity
        {
            Id = RoleConstants.Customer,
            Name = "RoleName"
        };
        var customer = new CustomerEntity
        {
            FirstName = "CustomerFirstName",
            LastName = "CustomerLastName",
            Email = "test@gmail.com",
            PasswordHash = "hashpassword",
            Role = role
        };
        
        _customerRepository
            .ExistsAsync(Arg.Any<GetCustomerByEmailSpecification>(), Arg.Any<CancellationToken>())
            .Returns(false);
        
        _customerRepository
            .ExistsAsync(Arg.Any<GetCustomerByLoyaltyIdSpecification>(), Arg.Any<CancellationToken>())
            .Returns(true);
        
        _roleRepository
            .GetAsync(Arg.Any<GetRoleByIdSpecification>(), Arg.Any<CancellationToken>())
            .Returns(role);
        
        _mapper.Map<CustomerEntity>(request).Returns(customer);
        
            var exception = await Assert.ThrowsAsync<ConflictException>(() =>
            _handler.Handle(command, CancellationToken.None));
        Assert.Equal(HandlerConstants.Customer.GeneratedLoyaltyIdAlreadyExistMessage, exception.Message);
    }
    
    [Fact]
    public async Task Handle_WhenPhoneNumberIsEmpty_CreatesCustomerWithoutCountryValidation()
    {
        var request = CreateValidCustomerRequest();
        request.PhoneNumber = null;
        var command = new CreateCustomerCommand(10, request);

        var role = new RoleEntity
        {
            Id = RoleConstants.Customer,
            Name = "RoleName"
        };
        var customer = new CustomerEntity
        {
            FirstName = "CustomerFirstName",
            LastName = "CustomerLastName",
            Email = "test@gmail.com",
            PasswordHash = "hashpassword",
            Role = role
        };
        var response = new CreateCustomerResponse
        {
            FirstName = "FistName",
            LastName = "LastName",
            Email = "test@gmail.com"
        };
        

        _customerRepository
            .ExistsAsync(Arg.Any<GetCustomerByEmailSpecification>(), Arg.Any<CancellationToken>())
            .Returns(false);

        _customerRepository
            .ExistsAsync(Arg.Any<GetCustomerByLoyaltyIdSpecification>(), Arg.Any<CancellationToken>())
            .Returns(false);

        _roleRepository
            .GetAsync(Arg.Any<GetRoleByIdSpecification>(), Arg.Any<CancellationToken>())
            .Returns(role);

        _mapper.Map<CustomerEntity>(request).Returns(customer);
        _mapper.Map<CreateCustomerResponse>(customer).Returns(response);
        
        var result = await _handler.Handle(command, CancellationToken.None);

        Assert.Same(response, result);
        Assert.Null(result.Country);

        Assert.Equal(role.Id, customer.RoleId);
        Assert.Equal(command.ShopId, customer.ShopId);
        Assert.True(customer.IsActive);
        Assert.False(string.IsNullOrWhiteSpace(customer.PasswordHash));
        Assert.False(string.IsNullOrWhiteSpace(customer.LoyaltyId));

        await _countryValidator.DidNotReceive()
            .ValidateAsync(Arg.Any<CountryValidationModel>(), Arg.Any<CancellationToken>(), Arg.Any<CountryValidationOptions>());

        await _countryRepository.DidNotReceive()
            .GetAsync(Arg.Any<GetCountryByIdSpecification>(), Arg.Any<CancellationToken>());

        await _customerRepository.Received(1).AddAsync(customer, Arg.Any<CancellationToken>());
        _transaction.Received(1).Commit();
    }

    [Fact]
    public async Task Handle_Should_CreateCustomer_WhenValidRequest()
    {
        var request = CreateValidCustomerRequest();
        var command = new CreateCustomerCommand(10, request);
        var role = new RoleEntity
        {
            Id = RoleConstants.Customer,
            Name = "RoleName"
        };
        var customer = new CustomerEntity
        {
            FirstName = "CustomerFirstName",
            LastName = "CustomerLastName",
            Email = "test@gmail.com",
            PasswordHash = "hashpassword",
            Role = role
        };
        var response = new CreateCustomerResponse
        {
            FirstName = "FistName",
            LastName = "LastName",
            Email = "test@gmail.com"
        };
        var country = new CountryEntity
        {
            Id = 3,
            PhoneCode = "+994",
            Name = "Azerbaijan",
            PhoneMask = "test",
            ZipCodeMask = "test"
        };
        
        var countryResponse = new CountryResponse
        {
            Name = "Azerbaijan",
            PhoneCode = "+994",
            PhoneMask = "test",
            ZipCodeMask = "test"
        };
        _customerRepository
            .ExistsAsync(Arg.Any<GetCustomerByEmailSpecification>(), Arg.Any<CancellationToken>())
            .Returns(false);

        _customerRepository
            .ExistsAsync(Arg.Any<GetCustomerByLoyaltyIdSpecification>(), Arg.Any<CancellationToken>())
            .Returns(false);

        _countryRepository
            .GetAsync(Arg.Any<GetCountryByIdSpecification>(), Arg.Any<CancellationToken>())
            .Returns(country);

        _roleRepository
            .GetAsync(Arg.Any<GetRoleByIdSpecification>(), Arg.Any<CancellationToken>())
            .Returns(role);
        _mapper.Map<CustomerEntity>(request).Returns(customer);
        _mapper.Map<CreateCustomerResponse>(customer).Returns(response);
        _mapper.Map<CountryResponse>(country).Returns(countryResponse);
        
        var result = await _handler.Handle(command, CancellationToken.None);

        Assert.Same(response, result);
        Assert.Same(countryResponse, result.Country);
        Assert.Equal("+994 1234456678", customer.PhoneNumber);

        await _countryValidator.Received(1).ValidateAsync(
            Arg.Is<CountryValidationModel>(x =>
                x.CountryId == request.CountryId &&
                x.PhoneNumber == request.PhoneNumber),
            Arg.Any<CancellationToken>(),
            Arg.Is<CountryValidationOptions>(x =>
                x.CheckForPhoneNumber &&
                !x.CheckForZipCode));

        await _countryRepository.Received(1)
            .GetAsync(Arg.Any<GetCountryByIdSpecification>(), Arg.Any<CancellationToken>());

        await _customerRepository.Received(1).AddAsync(customer, Arg.Any<CancellationToken>());
        _transaction.Received(1).Commit();
    }
    
    

    private CreateCustomerRequest CreateValidCustomerRequest()
    {
        var request = new CreateCustomerRequest
        {
            FirstName = "firstname",
            LastName = "lastname",
            Email = "test@gmail.com",
            CountryId = 3,
            PhoneNumber = "1234456678",
            Remarks = "Remarks"
        };
        return request;
    }
    
}