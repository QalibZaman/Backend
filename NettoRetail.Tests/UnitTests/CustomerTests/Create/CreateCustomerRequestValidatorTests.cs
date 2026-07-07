using FluentValidation.TestHelper;
using NettoRetail.API.Infrastructure.Validators;
using NettoRetail.Application.Models.Request.Customer;
using NettoRetail.Shared.Constants;

namespace NettoRetail.Tests.UnitTests.CustomerTests;

public class CreateCustomerRequestValidatorTests
{
    public readonly CreateCustomerRequestValidator _validator;
    public CreateCustomerRequestValidatorTests()
    { 
        _validator = new CreateCustomerRequestValidator();
        
    }

    [Theory]
    [InlineData("a", "First name must be between 2 and 30 characters long.")]
    [InlineData("aaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaa", "First name must be between 2 and 30 characters long.")]
    [InlineData("", ValidationErrorMessages.Customer.FirstNameRequiredErrorMessage)]
    [InlineData("Customer123", ValidationErrorMessages.Customer.FirstNameIsInvalidErrorMessage)]
    [InlineData(" Customer123 ", ValidationErrorMessages.Customer.FirstNameStartsWithWhitespacesErrorMessage)]
    public async Task Validator_ShouldFail_WhenFirstNameIsInvalid(string name, string expectedError)
    {
        var model = CreateValidCustomerRequest();
        model.FirstName = name;
        var result = await _validator.TestValidateAsync(model);
        result.ShouldHaveValidationErrorFor(x => x.FirstName).WithErrorMessage(expectedError);
    }
    [Theory]
    [InlineData("a", "Last name must be between 2 and 30 characters long.")]
    [InlineData("aaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaa", "Last name must be between 2 and 30 characters long.")]
    [InlineData("", ValidationErrorMessages.Customer.LastNameRequiredErrorMessage)]
    [InlineData("Customer123", ValidationErrorMessages.Customer.LastNameIsInvalidErrorMessage)]
    [InlineData(" Customer123 ", ValidationErrorMessages.Customer.LastNameStartsWithWhitespacesErrorMessage)]
    public async Task Validator_ShouldFail_WhenLastNameIsInvalid(string lastName, string expectedError)
    {
        var model = CreateValidCustomerRequest();
        model.LastName = lastName;
        var result = await _validator.TestValidateAsync(model);
        result.ShouldHaveValidationErrorFor(x => x.LastName).WithErrorMessage(expectedError);
    }

    [Theory]
    [InlineData("", ValidationErrorMessages.Customer.EmailRequiredErrorMessage)]
    [InlineData(null, ValidationErrorMessages.Customer.EmailRequiredErrorMessage)]
    [InlineData(" email123 ", ValidationErrorMessages.Customer.EmailInvalidErrorMessage)]
    public async Task Validator_ShouldFail_WhenEmailIsInvalid(string email, string expectedError)
    {
        var model = CreateValidCustomerRequest();
        model.Email = email;
        var result = await _validator.TestValidateAsync(model);
        result.ShouldHaveValidationErrorFor(x => x.Email).WithErrorMessage(expectedError);
    }
    [Fact]
    public async Task Validator_ShouldFail_WhenPhoneNumberIsNull()
    {
        var model = CreateValidCustomerRequest();
        model.PhoneNumber = null;
        var result = await _validator.TestValidateAsync(model);
        result.ShouldHaveValidationErrorFor(x => x.PhoneNumber).WithErrorMessage(ValidationErrorMessages.Customer.PhoneNumberRequiredErrorMessage);
        
    }

    [Fact]
    public async Task Validator_ShouldFail_WhenCountryIdIsInvalid()
    {
        var model = CreateValidCustomerRequest();
        model.CountryId = 0;
        var result = await _validator.TestValidateAsync(model);
        result.ShouldHaveValidationErrorFor(x => x.CountryId).WithErrorMessage(ValidationErrorMessages.Customer.CountryIdRequiredErrorMessage);
    }
    
    [Theory]
    [InlineData("a", "Remarks length must be between 2 and 100 characters.")]
    public async Task Validator_ShouldFail_WhenRemarksAreInvalid(string remarks, string expectedError)
    {
        var model = CreateValidCustomerRequest();
        model.Remarks = remarks;
        var result = await _validator.TestValidateAsync(model);
        result.ShouldHaveValidationErrorFor(x => x.Remarks).WithErrorMessage(expectedError);
    }
    
    
    
    private  CreateCustomerRequest CreateValidCustomerRequest()
    {
        var request = new CreateCustomerRequest
        {
            FirstName = "John",
            LastName = "Doe",
            Email = "test@gmail.com",
            CountryId = 1,
            PhoneNumber = "1234567890",
            Remarks = "This is a test"
        };
        return request;
    }
}