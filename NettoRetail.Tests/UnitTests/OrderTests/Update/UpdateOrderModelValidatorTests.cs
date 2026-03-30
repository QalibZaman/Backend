using FluentValidation.TestHelper;
using NettoRetail.API.Infrastructure.Validators;
using NettoRetail.Application.Models.Order;
using NettoRetail.Shared.Constants;

namespace NettoRetail.Tests.UnitTests.OrderTests.Update;

public class UpdateOrderModelValidatorTests
{
    private readonly UpdateOrderModelValidator _validator;
    public UpdateOrderModelValidatorTests()
    {
        _validator = new UpdateOrderModelValidator();
    }

    [Fact]
    public void Validator_Should_Succeed_When_Input_Is_Valid()
    {
        var model = SetupValidUpdateOrderModel();

        var result = _validator.TestValidate(model);

        result.ShouldNotHaveAnyValidationErrors();
    }

    [Fact]
    public void Validator_Should_Fail_When_ItemsToUpsertValue_Is_Less_Than_Zero()
    {
        var model = SetupValidUpdateOrderModel();
        model.ItemsToUpsert = new Dictionary<Guid, int>() { { Guid.NewGuid(), -12 } };

        var result = _validator.TestValidate(model);

        result.ShouldHaveValidationErrorFor(x => x.ItemsToUpsert)
            .WithErrorMessage(ValidationErrorMessages.Order.ItemGreaterThanZeroErrorMessage);
    }

    [Fact]
    public void Validator_Should_Fail_When_ItemsToUpsert_Intersepts_ItemsToDelete()
    {
        var guid = Guid.NewGuid();
        var model = SetupValidUpdateOrderModel();
        model.ItemsToDelete = new List<Guid> { guid };
        model.ItemsToUpsert = new Dictionary<Guid, int> { { guid, 1 } };

        var result = _validator.TestValidate(model);

        result.ShouldHaveValidationErrorFor(x => x.ItemsToUpsert)
            .WithErrorMessage(ValidationErrorMessages.Order.ItemUpsertDeleteConflictErrorMessage);
    }

    private UpdateOrderModel SetupValidUpdateOrderModel()
    {

        var model = new UpdateOrderModel 
        { 
            CustomerId = Guid.NewGuid(),
            ItemsToDelete = new List<Guid> 
            {
                Guid.NewGuid() 
            },
            ItemsToUpsert = new Dictionary<Guid, int> 
            {
                { Guid.NewGuid(), 1  },
            }
        };
        return model;
    }
}
