using Mapster;
using MapsterMapper;
using Microsoft.Extensions.DependencyInjection;
using NettoRetail.Application.MappingConfiguration;
using NettoRetail.Application.CQRS.Customers.Queries;
using NettoRetail.Application.Exceptions;
using NettoRetail.Domain.Entities;
using NettoRetail.Shared.Interfaces.Repositories;
using NettoRetail.Shared.Specifications;
using NSubstitute;
using NSubstitute.ExceptionExtensions;
using System.Linq.Expressions;

namespace NettoRetail.Tests.UnitTests.CustomerTests;
public class GetCustomerInfoQueryHandlerTests
{
    private readonly ICustomerRepository _customerRepository;

    public GetCustomerInfoQueryHandlerTests()
    {
        _customerRepository = Substitute.For<ICustomerRepository>();
    }

    private static IMapper CreateMapper()
    {
        var config = new TypeAdapterConfig();
        config.Apply(new MappingConfiguration());

        var services = new ServiceCollection();
        services.AddSingleton(config);
        services.AddScoped<IMapper, ServiceMapper>();

        var provider = services.BuildServiceProvider();
        return provider.GetRequiredService<IMapper>();
    }

    [Fact]
    public async Task Handle_ShouldThrowNotFound_WhenCustomerDoesNotExist()
    {
        // Arrange

        var handler = new GetCustomerInfoQueryHandler(
            _customerRepository,
            CreateMapper());
        var query = new GetCustomerInfoQuery("LOYALTY-404");

        _customerRepository
            .GetAsync(Arg.Any<Specification<CustomerEntity>>(), Arg.Any<CancellationToken>())
            .Returns((CustomerEntity?)null);

        // Act & Assert
        await Assert.ThrowsAsync<NotFoundException>(() => handler.Handle(query, CancellationToken.None));

        await _customerRepository.Received(1)
            .GetAsync(Arg.Any<Specification<CustomerEntity>>(), Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task Handle_ShouldReturnMappedResponse_WhenCustomerExists()
    {
        // Arrange
        var loyaltyId = "LOYALTY-123";
        var query = new GetCustomerInfoQuery(loyaltyId);

        var handler = new GetCustomerInfoQueryHandler(
            _customerRepository,
            CreateMapper());

        var customer = new CustomerEntity
        {
            Id = Guid.NewGuid(),
            LoyaltyId = loyaltyId,
            IsActive = true,
            IsDeleted = false,
            FirstName = "John",
            LastName = "Doe",
            Email = "john@doe.com",
            PasswordHash = "hash"
        };

        _customerRepository
            .GetAsync(Arg.Any<Specification<CustomerEntity>>(), Arg.Any<CancellationToken>())
            .Returns(customer);

        // Act
        var result = await handler.Handle(query, CancellationToken.None);

        // Assert
        Assert.NotNull(result);
        Assert.Equal(loyaltyId, result.LoyaltyId);
        Assert.Equal(customer.Id, result.AccountId);

        await _customerRepository.Received(1)
            .GetAsync(Arg.Any<Specification<CustomerEntity>>(), Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task Handle_ShouldPassCancellationToken_ToRepository()
    {
        // Arrange
        var loyaltyId = "LOYALTY-CT";
        var query = new GetCustomerInfoQuery(loyaltyId);

        var handler = new GetCustomerInfoQueryHandler(
            _customerRepository,
            CreateMapper());
        var cancellationToken = new CancellationToken();

        _customerRepository
            .GetAsync(Arg.Any<Specification<CustomerEntity>>(), cancellationToken)
            .Returns(new CustomerEntity
            {
                Id = Guid.NewGuid(),
                LoyaltyId = loyaltyId,
                IsActive = true,
                IsDeleted = false,
                FirstName = "John",
                LastName = "Doe",
                Email = "john@doe.com",
                PasswordHash = "hash"
            });

        // Act
        _ = await handler.Handle(query, cancellationToken);

        // Assert
        await _customerRepository.Received(1)
            .GetAsync(Arg.Any<Specification<CustomerEntity>>(), cancellationToken);
    }

    [Fact]
    public async Task Handle_ShouldMap_WhenAccountIsNull()
    {
        // Arrange
        var loyaltyId = "LOYALTY-NULL-ACC";

        var query = new GetCustomerInfoQuery(loyaltyId);

        var customer = new CustomerEntity
        {
            Id = Guid.NewGuid(),
            LoyaltyId = loyaltyId,
            IsActive = true,
            FirstName = "John",
            LastName = "Doe",
            Email = "john@doe.com",
            PasswordHash = "hash"
        };

        _customerRepository
            .GetAsync(Arg.Any<Specification<CustomerEntity>>(), Arg.Any<CancellationToken>())
            .Returns(customer);


        var handler = new GetCustomerInfoQueryHandler(
            _customerRepository,
            CreateMapper());

        // Act
        var result = await handler.Handle(query, CancellationToken.None);

        // Assert
        Assert.NotNull(result);
        Assert.Equal(loyaltyId, result.LoyaltyId);
    }




    [Fact]
    public async Task Handle_ShouldBubbleRepositoryExceptions()
    {
        // Arrange
        var query = new GetCustomerInfoQuery("LOYALTY-EX");

        var handler = new GetCustomerInfoQueryHandler(
            _customerRepository,
            CreateMapper());

        _customerRepository
            .GetAsync(Arg.Any<Specification<CustomerEntity>>(), Arg.Any<CancellationToken>())
            .Throws(new InvalidOperationException("DB error"));

        // Act & Assert
        await Assert.ThrowsAsync<InvalidOperationException>(() => handler.Handle(query, CancellationToken.None));
    }

    [Fact]
    public async Task Handle_ShouldBuildSpecification_WithCorrectLoyaltyFilter()
    {
        // Arrange
        var loyaltyId = "LOYALTY-SPEC";
        var query = new GetCustomerInfoQuery(loyaltyId);

        var handler = new GetCustomerInfoQueryHandler(
            _customerRepository,
            CreateMapper());

        Specification<CustomerEntity>? capturedSpec = null;

        var customer = new CustomerEntity
        {
            Id = Guid.NewGuid(),
            LoyaltyId = loyaltyId,
            IsActive = true,
            IsDeleted = false,
            FirstName = "John",
            LastName = "Doe",
            Email = "john@doe.com",
            PasswordHash = "hash"
        };

        _customerRepository
            .GetAsync(Arg.Do<Specification<CustomerEntity>>(s => capturedSpec = s),
                Arg.Any<CancellationToken>())
            .Returns(customer);

        // Act
        var result = await handler.Handle(query, CancellationToken.None);

        // Assert
        Assert.NotNull(result);
        Assert.NotNull(capturedSpec);

        var predicate = capturedSpec!.Criteria.Compile();

        var matchingCustomer = new CustomerEntity
        {
            LoyaltyId = loyaltyId.ToLower(),
            Id = Guid.NewGuid(),
            IsActive = true,
            FirstName = "John",
            LastName = "Doe",
            Email = "john@doe.com",
            PasswordHash = "hash"
        };

        var nonMatchingCustomer = new CustomerEntity
        {
            LoyaltyId = "SOME-OTHER",
            Id = Guid.NewGuid(),
            IsActive = true,
            FirstName = "John",
            LastName = "Doe",
            Email = "john@doe.com",
            PasswordHash = "hash"
        };

        Assert.True(predicate(matchingCustomer));
        Assert.False(predicate(nonMatchingCustomer));
    }

    [Fact]
    public async Task Handle_ShouldIncludeAccount_InSpecificationIncludes()
    {
        // Arrange
        var loyaltyId = "LOYALTY-INCLUDE";
        var query = new GetCustomerInfoQuery(loyaltyId);

        var handler = new GetCustomerInfoQueryHandler(
            _customerRepository,
            CreateMapper());

        Specification<CustomerEntity>? capturedSpec = null;

        var customer = new CustomerEntity
        {
            Id = Guid.NewGuid(),
            LoyaltyId = loyaltyId,
            IsActive = true,
            IsDeleted = false,
            FirstName = "John",
            LastName = "Doe",
            Email = "john@doe.com",
            PasswordHash = "hash"
        };

        _customerRepository
            .GetAsync(Arg.Do<Specification<CustomerEntity>>(s => capturedSpec = s),
                Arg.Any<CancellationToken>())
            .Returns(customer);

        // Act
        _ = await handler.Handle(query, CancellationToken.None);

        // Assert
        Assert.NotNull(capturedSpec);

        var includeNames = capturedSpec!.Includes
            .Select(GetMemberName)
            .ToList();

        Assert.Contains(nameof(CustomerEntity), includeNames);
        Assert.DoesNotContain(nameof(CustomerEntity.Order), includeNames);
    }

    [Fact]
    public async Task Handle_ShouldThrowNotFound_WhenLoyaltyIdIsEmpty()
    {
        // Arrange

        var handler = new GetCustomerInfoQueryHandler(
            _customerRepository,
            CreateMapper());
        var query = new GetCustomerInfoQuery("");

        _customerRepository
            .GetAsync(Arg.Any<Specification<CustomerEntity>>(), Arg.Any<CancellationToken>())
            .Returns((CustomerEntity?)null);

        // Act + Assert
        await Assert.ThrowsAsync<NotFoundException>(() =>
            handler.Handle(query, CancellationToken.None));
    }

    [Fact]
    public async Task Handle_ShouldThrowNotFound_WhenLoyaltyIdIsWhitespace()
    {
        // Arrange

        var handler = new GetCustomerInfoQueryHandler(
            _customerRepository,
            CreateMapper());
        var query = new GetCustomerInfoQuery("   ");

        _customerRepository
            .GetAsync(Arg.Any<Specification<CustomerEntity>>(), Arg.Any<CancellationToken>())
            .Returns((CustomerEntity?)null);

        // Act + Assert
        await Assert.ThrowsAsync<NotFoundException>(() =>
            handler.Handle(query, CancellationToken.None));
    }


    [Fact]
    public async Task Handle_ShouldReturnCorrectAccountId_InHappyPath()
    {
        // Arrange
        var accountId = Guid.NewGuid();
        var query = new GetCustomerInfoQuery("OK");

        var handler = new GetCustomerInfoQueryHandler(
            _customerRepository,
            CreateMapper());

        var customer = new CustomerEntity
        {
            Id = accountId,
            LoyaltyId = "OK",
            IsActive = true,
            FirstName = "Test",
            LastName = "User",
            Email = "test@mail.com",
            PasswordHash = "dummy",

        };

        _customerRepository
            .GetAsync(Arg.Any<Specification<CustomerEntity>>(), Arg.Any<CancellationToken>())
            .Returns(customer);

        // Act
        var result = await handler.Handle(query, CancellationToken.None);

        // Assert
        Assert.Equal(accountId, result.AccountId);
    }

    [Fact]
    public async Task Handle_ShouldNotDuplicateIncludes_WhenBuilderChainsMultipleTimes()
    {
        // Arrange
        var loyaltyId = "LOYALTY-INCLUDE";
        var query = new GetCustomerInfoQuery("ABC");

        var handler = new GetCustomerInfoQueryHandler(
            _customerRepository,
            CreateMapper());

        Specification<CustomerEntity>? capturedSpec = null;

        var customer = new CustomerEntity
        {
            Id = Guid.NewGuid(),
            LoyaltyId = loyaltyId,
            IsActive = true,
            IsDeleted = false,
            FirstName = "John",
            LastName = "Doe",
            Email = "john@doe.com",
            PasswordHash = "hash"
        };

        _customerRepository
            .GetAsync(Arg.Do<Specification<CustomerEntity>>(s => capturedSpec = s),
                Arg.Any<CancellationToken>())
            .Returns(customer);

        // Act
        _ = await handler.Handle(query, CancellationToken.None);

        // Assert
        Assert.NotNull(capturedSpec);

        var includeNames = capturedSpec!.Includes.Select(GetMemberName).ToList();

        Assert.Single(includeNames);
        Assert.Contains(nameof(CustomerEntity), includeNames);

        Assert.DoesNotContain(nameof(CustomerEntity.Order), includeNames);
    }

    [Fact]
    public async Task Handle_SpecificationShouldNotThrow_WhenCustomerLoyaltyIdIsNull()
    {
        // Arrange
        var query = new GetCustomerInfoQuery("ABC");

        var handler = new GetCustomerInfoQueryHandler(
            _customerRepository,
            CreateMapper());

        var customer = new CustomerEntity
        {
            Id = Guid.NewGuid(),
            LoyaltyId = null,
            IsActive = true,
            IsDeleted = false,
            FirstName = "John",
            LastName = "Doe",
            Email = "john@doe.com",
            PasswordHash = "hash"
        };

        Specification<CustomerEntity>? capturedSpec = null;

        _customerRepository
            .GetAsync(Arg.Do<Specification<CustomerEntity>>(s => capturedSpec = s),
                Arg.Any<CancellationToken>())
            .Returns(customer);

        // Act
        _ = await handler.Handle(query, CancellationToken.None);

        // Assert 
        Assert.NotNull(capturedSpec);

        var predicate = capturedSpec!.Criteria.Compile();

        var exception = Record.Exception(() =>
            predicate(new CustomerEntity
            {
                LoyaltyId = null,
                Id = Guid.NewGuid(),
                IsActive = true,
                IsDeleted = false,
                FirstName = "John",
                LastName = "Doe",
                Email = "john@doe.com",
                PasswordHash = "hash"
            })
        );

        Assert.Null(exception);
    }


    [Fact]
    public async Task Handle_ShouldThrowNotFound_WhenRepositoryReturnsNullForValidLoyalty()
    {
        // Arrange
        var query = new GetCustomerInfoQuery("VALID-ID");

        var handler = new GetCustomerInfoQueryHandler(
            _customerRepository,
            CreateMapper());

        _customerRepository
            .GetAsync(Arg.Any<Specification<CustomerEntity>>(), Arg.Any<CancellationToken>())
            .Returns((CustomerEntity?)null);

        // Act
        var ex = await Assert.ThrowsAsync<NotFoundException>(() =>
            handler.Handle(query, CancellationToken.None));

        // Assert
        Assert.Contains("no customer", ex.Message.ToLower());
    }


    private static string GetMemberName(Expression<Func<CustomerEntity, object>> expression)
    {
        return expression.Body switch
        {
            MemberExpression member => member.Member.Name,
            UnaryExpression unary when unary.Operand is MemberExpression member
                => member.Member.Name,
            _ => throw new InvalidOperationException("Unsupported include expression")
        };
    }
}
