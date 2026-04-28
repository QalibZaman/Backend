using FluentValidation.TestHelper;
using NettoRetail.API.Infrastructure.Validators;
using NettoRetail.Application.Models.Request.Staff;
using NettoRetail.Shared.Constants;

namespace NettoRetail.Tests.UnitTests.UsersTests.StaffTests;

public class UpdateStaffRequestValidatorTests
{
    private readonly UpdateStaffRequestValidator _validator;
    public UpdateStaffRequestValidatorTests()
    {
        _validator = new UpdateStaffRequestValidator();
    }

    [Fact]
    public async Task Validate_Should_Have_Error_When_No_Fields_Are_Provided()
    {
        // Arrange
        var request = new UpdateStaffRequest();

        // Act
        var result = await _validator.TestValidateAsync(request);

        // Assert
        result.ShouldHaveValidationErrorFor(x => x)
            .WithErrorMessage(ValidationErrorMessages.Staff.AtLeastOneFieldErrorMessage);
    }

    [Theory]
    [InlineData("a")]
    [InlineData("Verylongnameactuallywaytoolongforthisfield")]
    [InlineData("john")]
    [InlineData("John123")]
    public async Task Validate_Should_Have_Error_When_FirstName_Is_Invalid(string invalidName)
    {
        // Arrange
        var request = new UpdateStaffRequest { FirstName = invalidName };

        // Act
        var result = await _validator.TestValidateAsync(request);

        // Assert
        result.ShouldHaveAnyValidationError();
    }

    [Fact]
    public async Task Validate_Should_Have_Error_When_Email_Is_Invalid()
    {
        // Arrange
        var request = new UpdateStaffRequest { Email = "invalid-email-format" };

        // Act
        var result = await _validator.TestValidateAsync(request);

        // Assert
        result.ShouldHaveValidationErrorFor(x => x.Email)
            .WithErrorMessage(ValidationErrorMessages.Email.PatternErrorMessage);
    }

    [Fact]
    public async Task Validate_Should_Have_Error_When_RoleId_Is_Zero_Or_Negative()
    {
        // Arrange
        var request = new UpdateStaffRequest { RoleId = 0 };

        // Act
        var result = await _validator.TestValidateAsync(request);

        // Assert
        result.ShouldHaveValidationErrorFor(x => x.RoleId);
    }

    [Fact]
    public async Task Validate_Should_Not_Have_Error_When_FirstName_Is_Correctly_Formatted()
    {
        // Arrange
        var request = new UpdateStaffRequest { FirstName = "Test" };

        // Act
        var result = await _validator.TestValidateAsync(request);

        // Assert
        result.ShouldNotHaveValidationErrorFor(x => x.FirstName);
    }

    [Fact]
    public async Task Validate_Should_Not_Have_Error_When_Only_IsActive_Is_Provided()
    {
        // Arrange
        var request = new UpdateStaffRequest { IsActive = true };

        // Act
        var result = await _validator.TestValidateAsync(request);

        // Assert
        result.ShouldNotHaveAnyValidationErrors();
    }

    [Fact]
    public async Task Validate_Should_Not_Have_Error_When_All_Fields_Are_Valid()
    {
        // Arrange
        var request = new UpdateStaffRequest
        {
            FirstName = "Test",
            LastName = "Test",
            Email = "test@gmail.com",
            RoleId = 1,
            IsActive = true
        };

        // Act
        var result = await _validator.TestValidateAsync(request);

        // Assert
        result.ShouldNotHaveAnyValidationErrors();
    }
}
