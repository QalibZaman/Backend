using FluentValidation.TestHelper;
using NettoRetail.API.Infrastructure.Validators;
using NettoRetail.Application.Models.Request.Variation;
using static Microsoft.EntityFrameworkCore.DbLoggerCategory.Model;

namespace NettoRetail.Tests.UnitTests.VariationTests;

public class CreateVariationsRequestValidatorTests
{
    private readonly CreateVariationsRequestValidator _validator = new();

    [Fact]
    public void Validate_ShouldFail_WhenVariationUpcsAreDuplicated()
    {
        var model = new CreateVariationsRequest
        {
            InventoryItemId = Guid.NewGuid(),
            Variations =
            [
                new VariationModel { ProductName = "Variant_1", ProductUpc = "123456789012", IsActive = true },
                new VariationModel { ProductName = "Variant_2", ProductUpc = "123456789012", IsActive = false }
            ]
        };

        var result = _validator.TestValidate(model);

        result.ShouldHaveValidationErrorFor(x => x.Variations)
            .WithErrorMessage("Variation UPC values must be unique when provided."); 
    }

    [Fact]
    public void Validate_ShouldFail_WhenVariationProductNameHasInvalidFormat()
    {
        var model = new CreateVariationsRequest
        {
            InventoryItemId = Guid.NewGuid(),
            Variations =
            [
                new VariationModel { ProductName = "Bad name", ProductUpc = "123456789012", IsActive = true }
            ]
        };

        var result = _validator.TestValidate(model);

        result.ShouldHaveValidationErrorFor("Variations[0].ProductName");
    }

    [Fact]
    public void Validate_ShouldPass_WhenRequestIsValid()
    {
        var model = new CreateVariationsRequest
        {
            InventoryItemId = Guid.NewGuid(),
            Variations =
            [
                new VariationModel { ProductName = "Variant_1", ProductUpc = "123456789012", IsActive = true },
                new VariationModel { ProductName = "Variant_2", ProductUpc = "123456789013", IsActive = false }
            ]
        };

        var result = _validator.TestValidate(model);

        result.ShouldNotHaveAnyValidationErrors();
    }
}
