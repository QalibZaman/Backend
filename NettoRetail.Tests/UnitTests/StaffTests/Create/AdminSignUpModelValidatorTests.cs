using FluentValidation.TestHelper;
using NettoRetail.API.Infrastructure.Validators;
using NettoRetail.Application.Models.Admin;
using NettoRetail.Shared.Constants;

namespace NettoRetail.Tests.UnitTests.StaffTests.Create;

public class AdminSignUpModelValidatorTests
{
    private readonly AdminSignUpModelValidator  _validator = new();

    private AdminSignUpModel ValidSingUpModel()
    {
        var request = new AdminSignUpModel
        {
            FirstName = "Name",
            LastName = "Surname",
            Email = "email@email.com",
            Password = "12345Aa@"
        };
        return request;
    }
    [Theory]
    [InlineData("a", ValidationErrorMessages.Staff.FirstNameIsInvalidErrorMessage)]
    [InlineData("aaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaa", ValidationErrorMessages.Staff.FirstNameIsInvalidErrorMessage)]
    [InlineData("", ValidationErrorMessages.Staff.FirstNameIsInvalidErrorMessage)]
    [InlineData("Staff123", ValidationErrorMessages.Staff.FirstNameIsInvalidErrorMessage)]
    [InlineData(" Staff123 ", ValidationErrorMessages.Staff.FirstNameIsInvalidErrorMessage)]
    public async Task Validator_ShouldFail_WhenFirstNameIsInvalid(string name, string expectedError)
    {
        var model = ValidSingUpModel();
        model.FirstName = name;
        var result = await _validator.TestValidateAsync(model);
        result.ShouldHaveValidationErrorFor(x => x.FirstName).WithErrorMessage(expectedError);
    }
    
    [Theory]
    [InlineData("a", ValidationErrorMessages.Staff.LastNameIsInvalidErrorMessage)]
    [InlineData("aaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaa", ValidationErrorMessages.Staff.LastNameIsInvalidErrorMessage)]
    [InlineData("", ValidationErrorMessages.Staff.LastNameIsInvalidErrorMessage)]
    [InlineData("Staff123", ValidationErrorMessages.Staff.LastNameIsInvalidErrorMessage)]
    [InlineData(" Staff123 ", ValidationErrorMessages.Staff.LastNameIsInvalidErrorMessage)]
    public async Task Validator_ShouldFail_WhenLastNameIsInvalid(string lastName, string expectedError)
    {
        var model = ValidSingUpModel();
        model.LastName = lastName;
        var result = await _validator.TestValidateAsync(model);
        result.ShouldHaveValidationErrorFor(x => x.LastName).WithErrorMessage(expectedError);
    }
    [Theory]
    [InlineData("", ValidationErrorMessages.Staff.EmailIsInvalidErrorMessage)]
    [InlineData(null, ValidationErrorMessages.Staff.EmailIsInvalidErrorMessage)]
    [InlineData(" email123 ", ValidationErrorMessages.Staff.EmailIsInvalidErrorMessage)]
    public async Task Validator_ShouldFail_WhenEmailIsInvalid(string email, string expectedError)
    {
        var model = ValidSingUpModel();
        model.Email = email;
        var result = await _validator.TestValidateAsync(model);
        result.ShouldHaveValidationErrorFor(x => x.Email).WithErrorMessage(expectedError);
    }
    [Theory]
    [InlineData("", ValidationErrorMessages.Staff.PasswordIsRequiredErrorMessage)]
    [InlineData("12345", ValidationErrorMessages.Staff.PasswordIsInvalidErrorMessage)]
    [InlineData("123456Aa", ValidationErrorMessages.Staff.PasswordIsInvalidErrorMessage)]
    [InlineData("12345678@", ValidationErrorMessages.Staff.PasswordIsInvalidErrorMessage)]
    public async Task Validator_ShouldFail_WhenPasswordIsInvalid(string password, string expectedError)
    {
        var model = ValidSingUpModel();
        model.Password = password;
        var result = await _validator.TestValidateAsync(model);
        result.ShouldHaveValidationErrorFor(x => x.Password).WithErrorMessage(expectedError);
    }
}