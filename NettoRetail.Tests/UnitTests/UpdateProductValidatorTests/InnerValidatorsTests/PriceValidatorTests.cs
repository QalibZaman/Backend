using FluentValidation.TestHelper;
using NettoRetail.API.Infrastructure.Validators;
using NettoRetail.Application.Models.InventoryItem;
using NettoRetail.Shared.Constants;

namespace NettoRetail.Tests.UnitTests.UpdateProductValidatorTests;

public class PriceValidatorTests
{
    private readonly PriceValidator _validator;
    public PriceValidatorTests()
    {
        _validator = new PriceValidator();
    }

    [Fact]
    public void PassingEmptyModel_ShouldBeInvalid()
    {
        var model = new PriceModel();

        var result = _validator.TestValidate(model);

        result.ShouldHaveValidationErrorFor(m => m)
            .WithErrorMessage(ValidationErrorMessages.ANY_PROP_UPDATE_ERROR_MESSAGE)
            .Only();
    }

    [Fact]
    public void PassingInvalidPrice_ShouldReturnCorrectErrorMessage()
    {
        var model = new PriceModel()
        {
            Price = -1
        };

        var result = _validator.TestValidate(model);

        result.ShouldHaveValidationErrorFor(m => m.Price)
            .WithErrorMessage("Price must be 0 or non-negative value.")
            .Only();
    }

    [Fact]
    public void PassingValidPrice_ShouldBeValid()
    {
        var model = new PriceModel()
        {
            Price = 1000
        };

        var result = _validator.TestValidate(model);

        result.ShouldNotHaveAnyValidationErrors();
    }

    [Fact]
    public void PassingEnabledPromotionWithoutPromotionPriceAndStartDate_ShouldBeInvalid()
    {
        var model = new PriceModel()
        {
            IsPromoted = true
        };

        var result = _validator.TestValidate(model);

        result.ShouldHaveValidationErrorFor(m => m.PromotionPrice)
            .WithErrorMessage("Promotion price must be supplied when promotion is enabled.");

        result.ShouldHaveValidationErrorFor(m => m.StartDate)
            .WithErrorMessage("Start date must be provided when the promotion is enabled.");
    }

    [Fact]
    public void PassingEnabledPromotionWithCorrectPriceButWithoutStartDate_ShouldBeInvalid()
    {
        var model = new PriceModel()
        {
            IsPromoted = true,
            PromotionPrice = 999
        };

        var result = _validator.TestValidate(model);

        result.ShouldHaveValidationErrorFor(m => m.StartDate)
            .WithErrorMessage("Start date must be provided when the promotion is enabled.")
            .Only();
    }

    [Fact]
    public void PassingCorrectPromotionData_ShouldBeValid()
    {
        var model = new PriceModel()
        {
            IsPromoted = true,
            PromotionPrice = 999,
            StartDate = new DateOnly(2026, 2, 6)
        };

        var result = _validator.TestValidate(model);

        result.ShouldNotHaveAnyValidationErrors();
    }

    [Fact]
    public void PassingIncorrectEndDate_ShouldBeInvalid()
    {
        var model = new PriceModel()
        {
            IsPromoted = true,
            PromotionPrice = 999,
            StartDate = new DateOnly(2026, 2, 6),
            EndDate = new DateOnly(2023, 2, 6)
        };

        var result = _validator.TestValidate(model);

        result.ShouldHaveValidationErrorFor(m => m.EndDate)
            .WithErrorMessage("End date must be greater than the start date.")
            .Only();
    }

    [Fact]
    public void PassingCorrectFullPromotionInfo_ShouldBeValid()
    {
        var model = new PriceModel()
        {
            IsPromoted = true,
            PromotionPrice = 999,
            StartDate = new DateOnly(2026, 2, 6),
            EndDate = new DateOnly(2027, 2, 6)
        };

        var result = _validator.TestValidate(model);

        result.ShouldNotHaveAnyValidationErrors();
    }
}