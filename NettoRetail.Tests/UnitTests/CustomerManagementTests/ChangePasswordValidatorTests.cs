using FluentValidation.TestHelper;
using NettoRetail.API.Infrastructure.Validators;
using NettoRetail.Application.Models.Request.Password;
using NettoRetail.Shared.Constants;

namespace NettoRetail.Tests.UnitTests.CustomerManagementTests;

public class ChangePasswordValidatorTests
{
    private readonly ChangePasswordValidator _validator;

    public ChangePasswordValidatorTests()
    {
        _validator = new ChangePasswordValidator();
    }

    [Fact]
    public void Validator_ShouldPass_WhenRequestIsValid()
    {
        // Arrange
        var request = new ChangePasswordRequest("OldOne123!", "NewOne123!", "NewOne123!");

        // Act
        var result = _validator.TestValidate(request);

        // Assert
        result.ShouldNotHaveAnyValidationErrors();
    }

    [Fact]
    public void Validator_ShouldReturnMismatchError_WhenPasswordsDoNotMatch()
    {
        // Arrange
        var request = new ChangePasswordRequest("OldOne123!", "NewOne123!", "Different123!");

        // Act
        var result = _validator.TestValidate(request);

        // Assert
        result.ShouldHaveValidationErrorFor(x => x.ConfirmPassword)
            .WithErrorMessage(ValidationErrorMessages.Password.PasswordConfirmationMismatchErrorMessage);
    }

    [Fact]
    public void Validator_ShouldReturnMinLengthError_WhenNewPasswordIsTooShort()
    {
        // Arrange
        var request = new ChangePasswordRequest("OldOne123!", "Ab1!", "Ab1!");

        // Act
        var result = _validator.TestValidate(request);

        // Assert
        result.ShouldHaveValidationErrorFor(x => x.NewPassword)
            .WithErrorMessage(ValidationErrorMessages.Password.PasswordSize);
    }
}
