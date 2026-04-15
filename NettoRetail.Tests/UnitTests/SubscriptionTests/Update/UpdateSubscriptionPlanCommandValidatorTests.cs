using FluentValidation;
using FluentValidation.TestHelper;
using NettoRetail.API.Infrastructure.Validators;
using NettoRetail.Application.CQRS.Subscriptions.Commands;

namespace NettoRetail.Tests.UnitTests.SubscriptionTests.Update;

public class UpdateSubscriptionPlanCommandValidatorTests
{
    private readonly UpdateSubscriptionPlanCommandValidator _validator;
    public UpdateSubscriptionPlanCommandValidatorTests()
    {
        var updateSubscriptionPlanRequestValidator = new InlineValidator<UpdateSubscriptionPlanRequest>();

        _validator = new UpdateSubscriptionPlanCommandValidator(updateSubscriptionPlanRequestValidator);
    }


    [Fact]
    public void Validate_ShouldHaveError_WhenPlanIdIsZero()
    {
        // Arrange
        var command = new UpdateSubscriptionPlanCommand(0, Guid.NewGuid(), new UpdateSubscriptionPlanRequest());

        // Act
        var result = _validator.TestValidate(command);

        // Assert
        Assert.False(result.IsValid);
        Assert.True(result.Errors.Count == 1);
        result.ShouldHaveValidationErrorFor(c => c.PlanId);
    }

    [Fact]
    public void Validate_ShouldNotHaveError_WhenPlanIdIsValid()
    {
        // Arrange
        var command = new UpdateSubscriptionPlanCommand(1, Guid.NewGuid(), new UpdateSubscriptionPlanRequest());

        // Act
        var result = _validator.TestValidate(command);

        // Assert
        Assert.True(result.IsValid);
        Assert.True(result.Errors.Count == 0);
        result.ShouldNotHaveAnyValidationErrors();
    }
}
