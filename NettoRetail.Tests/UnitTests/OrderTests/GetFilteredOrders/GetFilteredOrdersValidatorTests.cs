using FluentValidation.TestHelper;
using NettoRetail.API.Infrastructure.Validators;
using NettoRetail.Application.Models.Order;
using NettoRetail.Application.Models.Request;
using NettoRetail.Shared.Constants;

namespace NettoRetail.Tests.UnitTests.OrderTests.GetFilteredOrders;

public class GetFilteredOrdersValidatorTests
{
    private readonly GetFilteredOrdersValidator _validator = new GetFilteredOrdersValidator();

    [Fact]
    public void Validator_Should_Fail_When_Names_Have_Invalid_Characters()
    {
        // Arrange
        var model = new GetFilteredOrdersModel
        {
            CustomerFirstNames = new List<string> { "John123!", "ValidName" }
        };

        // Act
        var result = _validator.TestValidate(model);

        // Assert
        result.ShouldHaveAnyValidationError()
            .WithErrorMessage(ValidationErrorMessages.Name.PatternErrorMessage);
    }

    [Fact]
    public void Validator_Should_Fail_When_Email_Is_Invalid()
    {
        // Arrange
        var model = new GetFilteredOrdersModel
        {
            CustomerEmails = new List<string> { "not-an-email" }
        };

        // Act
        var result = _validator.TestValidate(model);

        // Assert
        result.ShouldHaveAnyValidationError()
            .WithErrorMessage(ValidationErrorMessages.Email.PatternErrorMessage);
    }

    [Fact]
    public void Validator_Should_Fail_When_OrderId_Is_Zero_Or_Less()
    {
        // Arrange
        var model = new GetFilteredOrdersModel
        {
            OrderIds = new List<int> { 0, -5 }
        };

        // Act
        var result = _validator.TestValidate(model);

        // Assert
        result.ShouldHaveValidationErrorFor(x => x.OrderIds);
    }

    [Fact]
    public void Validator_Should_Fail_When_StartDate_Is_After_EndDate()
    {
        // Arrange
        var model = new GetFilteredOrdersModel
        {
            StartDate = DateTime.UtcNow.AddDays(1),
            EndDate = DateTime.UtcNow
        };

        // Act
        var result = _validator.TestValidate(model);

        // Assert
        result.ShouldHaveValidationErrorFor(x => x.StartDate)
            .WithErrorMessage(ValidationErrorMessages.Common.InvalidDateRangeErrorMessage);
    }

    [Fact]
    public void Validator_Should_Fail_When_SortingField_Is_Empty()
    {
        // Arrange
        var model = new GetFilteredOrdersModel
        {
            SortingOptions = new List<SortingOption>
            {
                new SortingOption { Field = "" }
            }
        };

        // Act
        var result = _validator.TestValidate(model);

        // Assert
        result.ShouldHaveAnyValidationError()
            .WithErrorMessage(ValidationErrorMessages.Sort.SortingFieldRequiredMessage);
    }

    [Fact]
    public void Validator_Should_Pass_When_Model_Is_Valid()
    {
        // Arrange
        var model = new GetFilteredOrdersModel
        {
            CustomerFirstNames = new List<string> { "John", "Jane" },
            CustomerEmails = new List<string> { "test@example.com" },
            OrderIds = new List<int> { 101, 102 },
            StartDate = DateTime.UtcNow.AddDays(-7),
            EndDate = DateTime.UtcNow,
            SortingOptions = new List<SortingOption>
            {
                new SortingOption { Field = "CreationDate", IsDescending = true }
            }
        };

        // Act
        var result = _validator.TestValidate(model);

        // Assert
        result.ShouldNotHaveAnyValidationErrors();
    }

    [Theory]
    [InlineData("ValidName")]
    [InlineData("Name-With-Hyphen")]
    [InlineData("Name With Space")]
    public void Validator_Should_Pass_For_Valid_Latin_Text(string validName)
    {
        // Arrange
        var model = new GetFilteredOrdersModel { Creators = new List<string> { validName } };

        // Act
        var result = _validator.TestValidate(model);

        // Assert
        result.ShouldNotHaveValidationErrorFor(x => x.Creators);
    }
}
