using FluentValidation.TestHelper;
using NettoRetail.API.Infrastructure.Validators;
using NettoRetail.Application.Models.Request.Customer;

namespace NettoRetail.Tests.UnitTests.CustomerManagementTests;

public class UpdateCustomerRequestValidatorTests
{
    private readonly UpdateCustomerRequestValidator _validator = new();

    [Fact]
    public async Task Validate_ShouldNotHaveError_WhenRemarksAreNull()
    {
        var request = CreateRequest();
        request.Remarks = null;

        var result = await _validator.TestValidateAsync(request);

        result.ShouldNotHaveValidationErrorFor(x => x.Remarks);
    }

    [Fact]
    public async Task Validate_ShouldHaveError_WhenRemarksAreTooShort()
    {
        var request = CreateRequest();
        request.Remarks = "A";

        var result = await _validator.TestValidateAsync(request);

        result.ShouldHaveValidationErrorFor(x => x.Remarks);
    }

    private static UpdateCustomerRequest CreateRequest()
    {
        return new UpdateCustomerRequest
        {
            FirstName = "John",
            LastName = "Doe",
            Email = "john@example.com",
            CountryId = 21,
            PhoneNumber = "123 456 789",
            Remarks = "Valid remarks",
            IsActive = true
        };
    }
}
