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

    [Fact]
    public async Task Handle_ShouldThrowNotFound_WhenCustomerDoesNotExist()
    {
        // Arrange
        var handler = new GetCustomerInfoQueryHandler(_customerRepository);
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
        var handler = new GetCustomerInfoQueryHandler(_customerRepository);

        var customer = new CustomerEntity
        {
            AccountId = Guid.NewGuid(),
            LoyaltyId = loyaltyId,
            IsActive = true,
            IsDeleted = false,
        };

        _customerRepository
            .GetAsync(Arg.Any<Specification<CustomerEntity>>(), Arg.Any<CancellationToken>())
            .Returns(customer);

        // Act
        var result = await handler.Handle(query, CancellationToken.None);

        // Assert
        Assert.NotNull(result);
        Assert.Equal(loyaltyId, result.LoyaltyId);
        Assert.Equal(customer.AccountId, result.AccountId);

        await _customerRepository.Received(1)
            .GetAsync(Arg.Any<Specification<CustomerEntity>>(), Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task Handle_ShouldPassCancellationToken_ToRepository()
    {
        // Arrange
        var loyaltyId = "LOYALTY-CT";
        var query = new GetCustomerInfoQuery(loyaltyId);
        var handler = new GetCustomerInfoQueryHandler(_customerRepository);
        var cancellationToken = new CancellationToken();

        _customerRepository
            .GetAsync(Arg.Any<Specification<CustomerEntity>>(), cancellationToken)
            .Returns(new CustomerEntity
            {
                AccountId = Guid.NewGuid(),
                LoyaltyId = loyaltyId,
                IsActive = true,
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
        var handler = new GetCustomerInfoQueryHandler(_customerRepository);

        var customer = new CustomerEntity
        {
            AccountId = Guid.NewGuid(),
            LoyaltyId = loyaltyId,
            IsActive = true,
            Account = null!
        };

        _customerRepository
            .GetAsync(Arg.Any<Specification<CustomerEntity>>(), Arg.Any<CancellationToken>())
            .Returns(customer);

        // Act
        var result = await handler.Handle(query, CancellationToken.None);

        // Assert
        Assert.NotNull(result);
        Assert.Equal(loyaltyId, result.LoyaltyId);

        Assert.Null(result.FirstName);
        Assert.Null(result.LastName);
        Assert.Null(result.Email);
    }



    [Fact]
    public async Task Handle_ShouldBubbleRepositoryExceptions()
    {
        // Arrange
        var query = new GetCustomerInfoQuery("LOYALTY-EX");
        var handler = new GetCustomerInfoQueryHandler(_customerRepository);

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
        var handler = new GetCustomerInfoQueryHandler(_customerRepository);

        Specification<CustomerEntity>? capturedSpec = null;

        var customer = new CustomerEntity
        {
            AccountId = Guid.NewGuid(),
            LoyaltyId = loyaltyId,
            IsActive = true,
            IsDeleted = false,
            Account = null!
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
            AccountId = Guid.NewGuid(),
            IsActive = true,
            Account = null!
        };

        var nonMatchingCustomer = new CustomerEntity
        {
            LoyaltyId = "SOME-OTHER",
            AccountId = Guid.NewGuid(),
            IsActive = true,
            Account = null!
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
        var handler = new GetCustomerInfoQueryHandler(_customerRepository);

        Specification<CustomerEntity>? capturedSpec = null;

        var customer = new CustomerEntity
        {
            AccountId = Guid.NewGuid(),
            LoyaltyId = loyaltyId,
            IsActive = true,
            IsDeleted = false,
            Account = null!
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

        Assert.Contains(nameof(CustomerEntity.Account), includeNames);
        Assert.DoesNotContain(nameof(CustomerEntity.Order), includeNames);
    }

    [Fact]
    public async Task Handle_ShouldThrowNotFound_WhenLoyaltyIdIsEmpty()
    {
        // Arrange
        var handler = new GetCustomerInfoQueryHandler(_customerRepository);
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
        var handler = new GetCustomerInfoQueryHandler(_customerRepository);
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
        var handler = new GetCustomerInfoQueryHandler(_customerRepository);

        var customer = new CustomerEntity
        {
            AccountId = accountId,
            LoyaltyId = "OK",
            IsActive = true,
            Account = new AccountEntity
            {
                FirstName = "Test",
                LastName = "User",
                Email = "test@mail.com",
                PasswordHash = "dummy",   
            }

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
        var query = new GetCustomerInfoQuery("ABC");
        var handler = new GetCustomerInfoQueryHandler(_customerRepository);

        Specification<CustomerEntity>? capturedSpec = null;

        var customer = new CustomerEntity
        {
            AccountId = Guid.NewGuid(),
            LoyaltyId = "ABC",
            IsActive = true,
            Account = null!
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
        Assert.Contains(nameof(CustomerEntity.Account), includeNames);

        Assert.DoesNotContain(nameof(CustomerEntity.Order), includeNames);
    }

    [Fact]
    public async Task Handle_SpecificationShouldNotThrow_WhenCustomerLoyaltyIdIsNull()
    {
        // Arrange
        var query = new GetCustomerInfoQuery("ABC");
        var handler = new GetCustomerInfoQueryHandler(_customerRepository);

        var customer = new CustomerEntity
        {
            AccountId = Guid.NewGuid(),
            LoyaltyId = null,  
            IsActive = true,
            Account = null!
        };

        Specification<CustomerEntity>? capturedSpec = null;

        _customerRepository
            .GetAsync(Arg.Do<Specification<CustomerEntity>>(s => capturedSpec = s),
                Arg.Any<CancellationToken>())
            .Returns(customer);

        // Act
        var result = await handler.Handle(query, CancellationToken.None);

        // Assert
        Assert.NotNull(capturedSpec);

        var predicate = capturedSpec!.Criteria.Compile();

        var exception = Record.Exception(() =>
            predicate(new CustomerEntity { LoyaltyId = null, AccountId = Guid.NewGuid(), Account = null! })
        );

        Assert.Null(exception);
    }

    [Fact]
    public async Task Handle_ShouldThrowNotFound_WhenRepositoryReturnsNullForValidLoyalty()
    {
        // Arrange
        var query = new GetCustomerInfoQuery("VALID-ID");
        var handler = new GetCustomerInfoQueryHandler(_customerRepository);

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
