using FluentValidation.TestHelper;
using Microsoft.VisualBasic;
using NettoRetail.API.Infrastructure.Validators;
using NettoRetail.Application.CQRS.Payments.Commands;
using NettoRetail.Application.Models.Payments;
using NettoRetail.Shared.Constants;

namespace NettoRetail.Tests.UnitTests.PaymentTests.AddPayment;

public class AddPaymentCommandValidatorTests
{
    private readonly AddPaymentCommandValidator _validator;
    public AddPaymentCommandValidatorTests()
    {
        _validator = new AddPaymentCommandValidator();
    }

    [Fact]
    public void Validator_Should_Fail_When_PaymentMethod_Is_Invalid()
    {
        var model = new AddPaymentCommand(Guid.NewGuid(), 5, "InvalidMethod");
        var result = _validator.TestValidate(model);

        result.ShouldHaveValidationErrorFor(x => x.PaymentMethodId)
            .WithErrorMessage(ValidationErrorMessages.Payment.InvalidPaymentMethodIdFormat);

        Assert.True(result.Errors.Count == 1);
    }

    [Fact]
    public void Validator_Should_Pass_When_PaymentMethod_Is_Valid()
    {
        var model = new AddPaymentCommand(Guid.NewGuid(), 5, "pm_card_mastercard");
        var result = _validator.TestValidate(model);

        result.ShouldNotHaveValidationErrorFor(x => x.PaymentMethodId);

        Assert.True(result.Errors.Count == 0);
    }

    [Fact]
    public void Validator_Should_Fail_When_AccountId_Is_Empty()
    {
        var model = new AddPaymentCommand(Guid.Empty, 5, "pm_card_mastercard");
        var result = _validator.TestValidate(model);

        result.ShouldHaveValidationErrorFor(x => x.AccountId)
            .WithErrorMessage(ValidationErrorMessages.Payment.IdNotEmpty);

        Assert.True(result.Errors.Count == 1);
    }

    [Fact]
    public void Validator_Should_Pass_When_AccountId_Is_Valid()
    {
        var model = new AddPaymentCommand(Guid.NewGuid(), 5, "pm_card_mastercard");
        var result = _validator.TestValidate(model);

        result.ShouldNotHaveValidationErrorFor(x => x.AccountId);

        Assert.True(result.Errors.Count == 0);
    }

    [Fact]
    public void Validator_Should_Fail_When_Both_AccountId_And_PaymentMethod_Are_Invalid()
    {
        var model = new AddPaymentCommand(Guid.Empty, 5, "InvalidMethod");
        var result = _validator.TestValidate(model);

        result.ShouldHaveValidationErrorFor(x => x.AccountId)
            .WithErrorMessage(ValidationErrorMessages.Payment.IdNotEmpty);

        result.ShouldHaveValidationErrorFor(x => x.PaymentMethodId)
            .WithErrorMessage(ValidationErrorMessages.Payment.InvalidPaymentMethodIdFormat);

        Assert.True(result.Errors.Count == 2);
    }

    [Fact]
    public void Validator_Should_Pass_When_Both_AccountId_And_PaymentMethod_Are_Valid()
    {
        var model = new AddPaymentCommand(Guid.NewGuid(), 5, "pm_card_mastercard");
        var result = _validator.TestValidate(model);

        result.ShouldNotHaveValidationErrorFor(x => x.AccountId);
        result.ShouldNotHaveValidationErrorFor(x => x.PaymentMethodId);

        Assert.True(result.Errors.Count == 0);
    }

    [Fact]
    public void Validator_Should_Fail_When_PaymentMethodId_Is_Empty()
    {
        var model = new AddPaymentCommand(Guid.NewGuid(), 5, string.Empty);
        var result = _validator.TestValidate(model);

        result.ShouldHaveValidationErrorFor(x => x.PaymentMethodId)
            .WithErrorMessage(ValidationErrorMessages.Payment.TokenNotEmpty);

        Assert.True(result.Errors.Count == 1);
    }
}