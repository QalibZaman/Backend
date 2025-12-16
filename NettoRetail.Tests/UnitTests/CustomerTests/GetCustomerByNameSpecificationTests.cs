using NettoRetail.DataAccess.Specifications.Customer;
using NettoRetail.Domain.Entities;

namespace NettoRetail.Tests.UnitTests.CustomerTests;

public class GetCustomerByNameSpecificationTests
{
    [Fact]
    public void ReturnsTrue_When_NameIsNullOrWhitespace()
    {
        // Arrange
        var specNull = new GetCustomerByNameSpecification(null);
        var specEmpty = new GetCustomerByNameSpecification("");
        var specSpaces = new GetCustomerByNameSpecification("   ");

        var predicateNull = specNull.Criteria.Compile();
        var predicateEmpty = specEmpty.Criteria.Compile();
        var predicateSpaces = specSpaces.Criteria.Compile();

        var customer = CreateCustomer("John", "Doe");

        // Assert
        Assert.True(predicateNull(customer));
        Assert.True(predicateEmpty(customer));
        Assert.True(predicateSpaces(customer));
    }

    [Fact]
    public void UsesOnlyFirstTwoWords_When_MoreThanTwoAreProvided()
    {
        // Arrange
        var spec = new GetCustomerByNameSpecification("John Michael Doe");
        var predicate = spec.Criteria.Compile();

        var customer = CreateCustomer("John", "Michael Doe");
        var customer1 = CreateCustomer("John Michael", "Doe");

        // Act
        var result = predicate(customer);
        var result1 = predicate(customer1);

        // Assert
        Assert.True(result);
        Assert.True(result1);
    }

    [Fact]
    public void SupportsContainsMatch_For_TwoWordQuery()
    {
        // Arrange
        var spec = new GetCustomerByNameSpecification("jo do");
        var predicate = spec.Criteria.Compile();

        var customer = CreateCustomer("John", "Doe");

        // Assert
        Assert.True(predicate(customer));
    }

    [Fact]
    public void Throws_When_Account_IsNull()
    {
        // Arrange
        var spec = new GetCustomerByNameSpecification("john");
        var predicate = spec.Criteria.Compile();

        var customer = new CustomerEntity
        {
            AccountId = Guid.NewGuid(),
            Account = null!,
            IsActive = true
        };

        // Act & Assert
        Assert.Throws<NullReferenceException>(() => predicate(customer));
    }

    [Fact]
    public void MatchesSingleTerm_Against_FirstOrLastName()
    {
        // Arrange
        var spec = new GetCustomerByNameSpecification("john");
        var predicate = spec.Criteria.Compile();

        var customer1 = CreateCustomer("John", "Smith");
        var customer2 = CreateCustomer("Alice", "Johnson");
        var customer3 = CreateCustomer("Alice", "Brown");

        // Assert
        Assert.True(predicate(customer1)); 
        Assert.True(predicate(customer2)); 
        Assert.False(predicate(customer3));  
    }

    [Fact]
    public void IsCaseInsensitive()
    {
        // Arrange
        var spec = new GetCustomerByNameSpecification("jOhN");
        var predicate = spec.Criteria.Compile();

        var customer = CreateCustomer("JOHN", "DOE");

        // Act
        var result = predicate(customer);

        // Assert
        Assert.True(result);
    }

    [Fact]
    public void MatchesTwoTerms_Against_FirstAndLastName_InAnyOrder()
    {
        // Arrange
        var spec = new GetCustomerByNameSpecification("john doe");
        var predicate = spec.Criteria.Compile();

        var customerExact = CreateCustomer("John", "Doe");
        var customerReversed = CreateCustomer("Doe", "John");
        var customerPartial = CreateCustomer("John", "Smith");

        // Assert
        Assert.True(predicate(customerExact));
        Assert.True(predicate(customerReversed));
        Assert.False(predicate(customerPartial));
    }

    [Fact]
    public void TrimsInput_And_IgnoresExtraSpaces()
    {
        // Arrange
        var spec = new GetCustomerByNameSpecification("   john   doe   ");
        var predicate = spec.Criteria.Compile();

        var customer = CreateCustomer("John", "Doe");

        // Act
        var result = predicate(customer);

        // Assert
        Assert.True(result);
    }

    [Fact]
    public void ReturnsTrue_When_InputSplitsToNoParts()
    {
        // Arrange
        var spec = new GetCustomerByNameSpecification("    ");
        var predicate = spec.Criteria.Compile();

        var customer = CreateCustomer("John", "Doe");

        // Act
        var result = predicate(customer);

        // Assert
        Assert.True(result);
    }

    private static CustomerEntity CreateCustomer(string firstName, string lastName)
    {
        var accountId = Guid.NewGuid();

        return new CustomerEntity
        {
            AccountId = accountId,
            Account = new AccountEntity
            {
                Id = accountId,
                FirstName = firstName,
                LastName = lastName,
                Email = $"{firstName.ToLower()}.{lastName.ToLower()}@test.com",
                PasswordHash = "hash123",
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
                }
            },
            IsActive = true
        };
    }
}
