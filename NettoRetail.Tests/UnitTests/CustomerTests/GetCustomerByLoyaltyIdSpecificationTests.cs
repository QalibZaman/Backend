using NettoRetail.DataAccess.Specifications.Customer;
using NettoRetail.Domain.Entities;

namespace NettoRetail.Tests.UnitTests.CustomerTests;

public class GetCustomerByLoyaltyIdSpecificationTests
{
    [Fact]
    public void ShouldMatch_WhenLoyaltyIdEqualsIgnoringCase()
    {
        var spec = new GetCustomerByLoyaltyIdSpecification("abc123");
        var predicate = spec.Criteria.Compile();

        var customer = CreateCustomer("ABC123");

        Assert.True(predicate(customer));
    }

    [Fact]
    public void ShouldNotMatch_WhenLoyaltyIdIsOnlyPartial()
    {
        var spec = new GetCustomerByLoyaltyIdSpecification("ABC123");
        var predicate = spec.Criteria.Compile();

        var customer = CreateCustomer("XXABC123YY");

        Assert.False(predicate(customer));
    }

    private static CustomerEntity CreateCustomer(string loyaltyId)
    {
        return new CustomerEntity
        {
            FirstName = "John",
            LastName = "Doe",
            Email = "john@example.com",
            PasswordHash = "hash",
            LoyaltyId = loyaltyId,
            RoleId = 1
        };
    }
}
