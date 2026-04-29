using FluentValidation;
using FluentValidation.TestHelper;
using NettoRetail.API.Infrastructure.Validators;
using NettoRetail.Application.Models.Request;
using NettoRetail.Application.Models.Request.Staff;
using NettoRetail.Shared.Constants;

namespace NettoRetail.Tests.UnitTests.UsersTests.StaffTests.Get;

public class GetFilteredStaffRequestValidatorTests
{
    private readonly GetFilteredStaffRequestValidator _validator;
    public GetFilteredStaffRequestValidatorTests()
    {
        var sortingOptionValidator = new InlineValidator<SortingOption>();

        _validator = new GetFilteredStaffRequestValidator(sortingOptionValidator);
    }

    [Fact]
    public void Validate_ValidRequest_ShouldPass()
    {
        //Arrange
        var request = ValidRequest();
        //Act
        var result = _validator.TestValidate(request);

        //Assert
        result.ShouldNotHaveAnyValidationErrors();
    }

    [Fact]
    public void Validate_FirstNameExceedsMaxLength_ShouldFail()
    {
        // Arrange
        var request = ValidRequest();
        request.FirstName = new string('A', 31);

        // Act
        var result = _validator.TestValidate(request);

        // Assert
        result.ShouldHaveValidationErrorFor(x => x.FirstName)
            .WithErrorMessage(ValidationErrorMessages.Staff.InvalidFirstNameErrorMessage);
    }

    [Fact]
    public void Validate_FirstNameContainsNonLetters_ShouldFail()
    {
        // Arrange
        var request = ValidRequest();
        request.FirstName = "John1";

        // Act
        var result = _validator.TestValidate(request);

        // Assert
        result.ShouldHaveValidationErrorFor(x => x.FirstName)
            .WithErrorMessage(ValidationErrorMessages.Staff.InvalidFirstNameErrorMessage);
    }

    [Fact]
    public void Validate_FirstNameHasLeadingWhitespace_ShouldFail()
    {
        // Arrange
        var request = ValidRequest();
        request.FirstName = " John";

        // Act
        var result = _validator.TestValidate(request);

        // Assert
        result.ShouldHaveValidationErrorFor(x => x.FirstName)
            .WithErrorMessage(ValidationErrorMessages.Staff.InvalidFirstNameErrorMessage);
    }

    [Fact]
    public void Validate_FirstNameStartsWithLowercase_ShouldFail()
    {
        // Arrange
        var request = ValidRequest();
        request.FirstName = "john";

        // Act
        var result = _validator.TestValidate(request);

        // Assert
        result.ShouldHaveValidationErrorFor(x => x.FirstName)
            .WithErrorMessage(ValidationErrorMessages.Staff.InvalidFirstNameErrorMessage);
    }

    [Fact]
    public void Validate_LastNameExceedsMaxLength_ShouldFail()
    {
        // Arrange
        var request = ValidRequest();
        request.LastName = new string('A', 31);

        // Act
        var result = _validator.TestValidate(request);

        // Assert
        result.ShouldHaveValidationErrorFor(x => x.LastName)
            .WithErrorMessage(ValidationErrorMessages.Staff.InvalidLastNameErrorMessage);
    }

    [Fact]
    public void Validate_LastNameContainsNonLetters_ShouldFail()
    {
        // Arrange
        var request = ValidRequest();
        request.LastName = "Doe1";

        // Act
        var result = _validator.TestValidate(request);

        // Assert
        result.ShouldHaveValidationErrorFor(x => x.LastName)
            .WithErrorMessage(ValidationErrorMessages.Staff.InvalidLastNameErrorMessage);
    }

    [Fact]
    public void Validate_LastNameStartsWithLowercase_ShouldFail()
    {
        // Arrange
        var request = ValidRequest();
        request.LastName = "doe";

        // Act
        var result = _validator.TestValidate(request);

        // Assert
        result.ShouldHaveValidationErrorFor(x => x.LastName)
            .WithErrorMessage(ValidationErrorMessages.Staff.InvalidLastNameErrorMessage);
    }

    [Fact]
    public void Validate_LastNameHasTrailingWhitespace_ShouldFail()
    {
        // Arrange
        var request = ValidRequest();
        request.LastName = "Doe ";

        // Act
        var result = _validator.TestValidate(request);

        // Assert
        result.ShouldHaveValidationErrorFor(x => x.LastName)
            .WithErrorMessage(ValidationErrorMessages.Staff.InvalidLastNameErrorMessage);
    }

    [Fact]
    public void Validate_EmailExceedsMaxLength_ShouldFail()
    {
        // Arrange
        var request = ValidRequest();
        request.Email = new string('a', 257) + "@b.com";

        // Act
        var result = _validator.TestValidate(request);

        // Assert
        result.ShouldHaveValidationErrorFor(x => x.Email);
    }

    [Fact]
    public void Validate_EmailInvalidFormat_ShouldFail()
    {
        // Arrange
        var request = ValidRequest();
        request.Email = "not-an-email";

        // Act
        var result = _validator.TestValidate(request);

        // Assert
        result.ShouldHaveValidationErrorFor(x => x.Email)
            .WithErrorMessage(ValidationErrorMessages.Common.InvalidEmailFormatErrorMessage);
    }

    [Fact]
    public void Validate_StatusIsInvalid_ShouldFail()
    {
        // Arrange
        var request = ValidRequest();
        request.Status = "InvalidStatus";

        // Act
        var result = _validator.TestValidate(request);

        // Assert
        result.ShouldHaveValidationErrorFor(x => x.Status)
            .WithErrorMessage(ValidationErrorMessages.Staff.InvalidStatusErrorMessage);
    }

    private GetFilteredStaffRequest ValidRequest()
    {
        return new GetFilteredStaffRequest
        {
            FirstName = "John",
            LastName = "Doe",
            Email = "john.doe@example.com",
            Status = "Active",
            SortingOptions = []
        };
    }
}
