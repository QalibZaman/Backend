using FluentValidation.TestHelper;
using NettoRetail.Application.CQRS.Payments.Queries;
using NettoRetail.Shared.Constants;

namespace NettoRetail.Tests.UnitTests.PaymentTests.Get;

public class GetPaymentMethodsQueryValidatorTests
{
    private readonly GetPaymentMethodsQueryValidator _validator = new();

    [Fact]
    public void Validator_Should_Fail_When_UserId_Is_Empty()
    {
        var query = new GetPaymentMethodsQuery(Guid.Empty, 1);

        var result = _validator.TestValidate(query);

        result.ShouldHaveValidationErrorFor(x => x.UserId)
            .WithErrorMessage(ValidationErrorMessages.Payment.IdNotEmpty);
    }

    [Fact]
    public void Validator_Should_Fail_When_ShopId_Is_Invalid()
    {
        var query = new GetPaymentMethodsQuery(Guid.NewGuid(), 0);

        var result = _validator.TestValidate(query);

        result.ShouldHaveValidationErrorFor(x => x.ShopId)
            .WithErrorMessage(ValidationErrorMessages.Shop.ShopIdRequiredErrorMessage);
    }

    [Fact]
    public void Validator_Should_Pass_When_Query_Is_Valid()
    {
        var query = new GetPaymentMethodsQuery(Guid.NewGuid(), 1);

        var result = _validator.TestValidate(query);

        result.ShouldNotHaveAnyValidationErrors();
    }
}
