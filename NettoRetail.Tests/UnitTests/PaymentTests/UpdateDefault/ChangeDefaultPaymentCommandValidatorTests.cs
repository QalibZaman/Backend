using FluentAssertions;
using FluentValidation.TestHelper;
using NettoRetail.API.Infrastructure.Validators;
using NettoRetail.Application.CQRS.Payments.Commands;
using NettoRetail.Shared.Constants;

namespace NettoRetail.Tests.UnitTests.PaymentTests.UpdateDefault;

public class ChangeDefaultPaymentCommandValidatorTests
{
    private readonly ChangeDefaultPaymentCommandValidator _validator;
    public ChangeDefaultPaymentCommandValidatorTests()
    {
        _validator = new ChangeDefaultPaymentCommandValidator();
    }

    [Fact]
    public void Validator_Should_Fail_When_StripePaymentMethod_Is_Invalid()
    {
        //Arrange
        var model = new ChangeDefaultPaymentCommand("paymentMethod", Guid.NewGuid());

        //Act
        var result = _validator.TestValidate(model);

        //Assert

        result.ShouldHaveValidationErrorFor(x => x.StripePaymentMethodId)
            .WithErrorMessage(ValidationErrorMessages.Payment.InvalidPaymentMethodIdFormat);

        Assert.True(result.Errors.Count == 1);
    }

    [Fact]
    public void Validator_ShouldNot_Fail_When_Command_Is_Valid()
    {
        //Arrange
        var model = new ChangeDefaultPaymentCommand("pm_23232233232", Guid.NewGuid());

        //Act
        var result = _validator.TestValidate(model);

        //Assert
        result.ShouldNotHaveAnyValidationErrors();

        Assert.True(result.Errors.Count == 0);
    }

    [Fact]
    public void Validator_Should_Fail_When_AccountId_Is_Empty()
    {
        //Arrange
        var model = new ChangeDefaultPaymentCommand("pm_23232233232", Guid.Empty);

        //Act
        var result = _validator.TestValidate(model);

        //Assert
        result.ShouldHaveValidationErrorFor(x => x.AccountId);

        Assert.True(result.Errors.Count == 1);
    }
}
