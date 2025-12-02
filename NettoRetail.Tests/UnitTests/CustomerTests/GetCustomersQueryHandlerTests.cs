using NettoRetail.Application.CQRS.Customers.Queries;
using NettoRetail.Application.Models.Request;
using NettoRetail.Application.Models.Request.Customer;
using NettoRetail.Domain.Entities;
using NettoRetail.Shared.Interfaces.Repositories;
using NettoRetail.Shared.Specifications;
using NSubstitute;

namespace NettoRetail.Tests.UnitTests.CustomerTests;

public class GetCustomersQueryHandlerTests
{
    private readonly ICustomerRepository _customerRepository;
    private readonly GetCustomersQueryHandler _handler;

    public GetCustomersQueryHandlerTests()
    {
        _customerRepository = Substitute.For<ICustomerRepository>();
        _handler = new GetCustomersQueryHandler(_customerRepository);
    }

    [Fact]
    public async Task ReturnsPaginatedResponse_ForValidRequest()
    {
        // Arrange
        var account = CreateAccount("John", "Doe", "john@test.com");
        var customers = new List<CustomerEntity>
        {
            new()
            {
                AccountId = account.Id,
                Account = account,
                IsActive = true
            }
        };

        _customerRepository
            .GetAllPaginatedAsync(
                Arg.Any<Specification<CustomerEntity>>(),
                Arg.Any<CancellationToken>())
            .Returns((customers, 1));

        var request = new GetCustomersQuery(
            new PaginationRequest { PageNumber = 1, PageSize = 10 },
            new GetCustomersRequest { Email = "test@example.com" });

        // Act
        var result = await _handler.Handle(request, CancellationToken.None);

        // Assert
        Assert.NotNull(result);
        Assert.NotNull(result.Items);
        Assert.Single(result.Items);
        Assert.Equal(1, result.TotalAmount);

        await _customerRepository.Received(1)
            .GetAllPaginatedAsync(
                Arg.Any<Specification<CustomerEntity>>(),
                Arg.Any<CancellationToken>());
    }

    private static AccountEntity CreateAccount(string firstName, string lastName, string email)
    {
        var accountId = Guid.NewGuid();

        var account = new AccountEntity
        {
            Id = accountId,
            FirstName = firstName,
            LastName = lastName,
            Email = email,
            PasswordHash = "hash",
            RoleId = 1,
            Role = new RoleEntity { Id = 1, Name = "User", Description = "Test" },
            CountryId = 1,
            Country = new CountryEntity
            {
                Id = 1,
                Name = "TestCountry",
                PhoneCode = "+1",
                PhoneMask = "###-###",
                ZipCodeMask = "#####"
            },
            FailedAuthAttempt = new FailedAuthAttemptEntity
            {
                Id = Guid.NewGuid(),
                AccountId = accountId,
                FailedAttempts = 0,
                LockoutEndTime = null,
                LastFailedTime = DateTime.UtcNow,
                Account = null!
            }
        };

        account.FailedAuthAttempt.Account = account;
        return account;
    }
}
