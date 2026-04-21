using FluentValidation.TestHelper;
using NettoRetail.API.Infrastructure.Validators;
using NettoRetail.Application.Models.Request.Variation;
using NettoRetail.Domain.Enums;

namespace NettoRetail.Tests.UnitTests.VariationTests;

public class UpdateVariationRequestValidatorTests
{
    private readonly UpdateVariationRequestValidator _validator = new();

    [Fact]
    public void Validate_ShouldAllowInheritedDescriptionWithoutDescriptionValue()
    {
        var model = CreateValidModel();
        model.ApplyDescriptionFromParent = true;
        model.Description = null;

        var result = _validator.TestValidate(model);

        result.ShouldNotHaveValidationErrorFor(x => x.Description);
    }

    [Fact]
    public void Validate_ShouldFail_WhenReservedAmountExceedsAvailableAmount()
    {
        var model = CreateValidModel();
        model.AvailableAmount = 2;
        model.ReservedAmount = 3;

        var result = _validator.TestValidate(model);

        result.ShouldHaveValidationErrorFor(x => x)
            .WithErrorMessage("Reserved amount cannot exceed the available amount.");
    }

    [Fact]
    public void Validate_ShouldFail_WhenCustomParameterHasInvalidValue()
    {
        var model = CreateValidModel();
        model.ApplyDetailsFromParent = false;
        model.CustomParameters =
        [
            new CustomDetailModel
            {
                Name = "Weight",
                TypeEnum = ProductDetailTypeEnum.Number,
                Unit = ProductDetailUnitEnum.kg,
                Value = "not-a-number",
                OrderIndex = 0
            }
        ];

        var result = _validator.TestValidate(model);

        result.ShouldHaveValidationErrorFor("CustomParameters[0].Value")
            .WithErrorMessage("Custom parameter value should be a valid number.");
    }

    private static UpdateVariationRequest CreateValidModel()
    {
        return new UpdateVariationRequest
        {
            InventoryItemId = Guid.NewGuid(),
            ProductUpc = "123456789012",
            IsActive = true,
            ApplyDescriptionFromParent = false,
            Description = "Variant description",
            ApplyPromotion = true,
            Price = 100m,
            PromotionPrice = 90m,
            ApplyImagesFromParent = true,
            Status = "available",
            AvailableAmount = 5,
            ReservedAmount = 1,
            OnHandAmount = 6,
            ApplyDetailsFromParent = true
        };
    }
}
