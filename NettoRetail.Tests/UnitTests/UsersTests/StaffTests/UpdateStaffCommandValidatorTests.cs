using FluentValidation;
using FluentValidation.TestHelper;
using NettoRetail.API.Infrastructure.Validators;
using NettoRetail.Application.CQRS.Users.Commands.Staff;
using NettoRetail.Application.Models.Request.Staff;

namespace NettoRetail.Tests.UnitTests.UsersTests.StaffTests;

public class UpdateStaffCommandValidatorTests
{
    private readonly InlineValidator<UpdateStaffRequest> _requestValidator;
    private readonly UpdateStaffCommandValidator _validator;
    public UpdateStaffCommandValidatorTests()
    {
        _requestValidator = new InlineValidator<UpdateStaffRequest>();
        _validator = new UpdateStaffCommandValidator(_requestValidator);
    }

    [Fact]
    public void Validator_Should_Fail_When_Id_Is_Empty()
    {
        // Arrange
        var command = new UpdateStaffCommand(Guid.Empty, new UpdateStaffRequest(), 1);
        // Act
        var result = _validator.TestValidate(command);
        // Assert
        result.ShouldHaveValidationErrorFor(x => x.Id)
            .WithErrorMessage("'Id' must not be empty.");
    }

    [Fact]
    public void Validator_Should_Fail_When_ShopId_Is_Invalid()
    {
        // Arrange
        var command = new UpdateStaffCommand(Guid.NewGuid(), new UpdateStaffRequest(), 0);
        // Act
        var result = _validator.TestValidate(command);
        // Assert
        result.ShouldHaveValidationErrorFor(x => x.ShopId)
            .WithErrorMessage("'Shop Id' must be greater than '0'.");
    }

    [Fact]
    public void Validator_Should_Pass_When_Data_Is_Valid()
    {
        // Arrange
        var command = new UpdateStaffCommand(Guid.NewGuid(), new UpdateStaffRequest { FirstName = "Test" }, 1);
        // Act
        var result = _validator.TestValidate(command);
        // Assert
        result.ShouldNotHaveAnyValidationErrors();
    }
}