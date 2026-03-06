using NettoRetail.API.Infrastructure.Validators;
using FluentValidation.TestHelper;
using NettoRetail.Application.Models.InventoryItem;
using NettoRetail.Shared.Constants;

namespace NettoRetail.Tests.UnitTests.UpdateProductValidatorTests;

public class GeneralInfoValidatorTests
{
    private readonly GeneralInfoValidator _validator;
    public GeneralInfoValidatorTests()
    {
        _validator = new GeneralInfoValidator();
    }

    [Fact]
    public void PassingEmptyModel_ShouldBeInvalid()
    {
        var model = new GeneralInfoModel();

        var result = _validator.TestValidate(model);

        result.ShouldHaveValidationErrorFor(m => m)
            .WithErrorMessage(ValidationErrorMessages.ANY_PROP_UPDATE_ERROR_MESSAGE);
    }

    [Fact]
    public void PassingShortProductName_ShouldBeInvalid()
    {
        var model = new GeneralInfoModel()
        {
            ProductName = "1"
        };

        var result = _validator.TestValidate(model);

        result.ShouldHaveValidationErrorFor(m => m.ProductName)
            .WithErrorMessage("Product name length must be at least 2 characters.")
            .Only();
    }

    [Fact]
    public void PassingLongProductName_ShouldBeInvalid()
    {
        var model = new GeneralInfoModel()
        {
            ProductName = "123456789012345678901234567890123456789012345678901234567890"
        };

        var result = _validator.TestValidate(model);

        result.ShouldHaveValidationErrorFor(m => m.ProductName)
            .WithErrorMessage("Product name length must be at max 50 characters.")
            .Only();
    }

    [Fact]
    public void PassingIncorrectProductName_ShouldBeInvalid()
    {
        var model = new GeneralInfoModel()
        {
            ProductName = " This Name contains space chars "
        };

        var result = _validator.TestValidate(model);

        result.ShouldHaveValidationErrorFor(m => m.ProductName)
            .WithErrorMessage("Allowed characters: letters (latin), digits and the following special characters: \"-\" and \"_\"")
            .Only();
    }

    [Fact]
    public void PassingCorrectProductName_ShouldBeValid()
    {
        var model = new GeneralInfoModel()
        {
            ProductName = "SuperProduct"
        };

        var result = _validator.TestValidate(model);

        result.ShouldNotHaveAnyValidationErrors();
    }

    [Fact]
    public void PassingIncorrectUpcLength_ShouldBeInvalid()
    {
        var model = new GeneralInfoModel()
        {
            ProductUpc = "12231321232132"
        };

        var result = _validator.TestValidate(model);

        result.ShouldHaveValidationErrorFor(m => m.ProductUpc)
            .WithErrorMessage("Product UPC must be 12 digits long.")
            .Only();
    }

    [Fact]
    public void PassingCorrectUpcLength_ShouldBeValid()
    {
        var model = new GeneralInfoModel()
        {
            ProductUpc = "123456789012"
        };

        var result = _validator.TestValidate(model);

        result.ShouldNotHaveAnyValidationErrors();
    }

    [Fact]
    public void PassingLongDescription_ShouldBeInvalid()
    {
        var model = new GeneralInfoModel()
        {
            Description = "Lorem ipsum dolor sit amet, consectetuer adipiscing elit. Aenean commodo ligula eget dolor. Aenean massa. Cum sociis natoque penatibus et magnis dis parturient montes, nascetur ridiculus mus. Donec quam felis, ultricies nec, pellentesque eu, pretium quis, sem. Nulla consequat massa quis enim. Donec pede justo, fringilla vel, aliquet nec, vulputate eget, arcu. In enim justo, rhoncus ut, imperdiet a, venenatis vitae, justo. Nullam dictum felis eu pede mollis pretium. Integer tincidunt. Cras dapibus"
        };

        var result = _validator.TestValidate(model);

        result.ShouldHaveValidationErrorFor(m => m.Description)
            .WithErrorMessage("Description must be 500 characters at max.")
            .Only();
    }

    [Fact]
    public void PassingDescriptionWithLeadingWhiteSpace_ShouldBeInvalid()
    {
        var model = new GeneralInfoModel()
        {
            Description = " Leading whitespace included."
        };

        var result = _validator.TestValidate(model);

        result.ShouldHaveValidationErrorFor(m => m.Description)
            .WithErrorMessage("Description must not have leading or trailing space characters.")
            .Only();
    }

    [Fact]
    public void PassingDescriptionWithTrailingWhiteSpace_ShouldBeInvalid()
    {
        var model = new GeneralInfoModel()
        {
            Description = "Leading whitespace included. "
        };

        var result = _validator.TestValidate(model);

        result.ShouldHaveValidationErrorFor(m => m.Description)
            .WithErrorMessage("Description must not have leading or trailing space characters.")
            .Only();
    }

    [Fact]
    public void PassingDescriptionWithIncorrectChars_ShouldBeInvalid()
    {
        var model = new GeneralInfoModel()
        {
            Description = "!!!! Contains wrong chars .???>>>>>"
        };

        var result = _validator.TestValidate(model);

        result.ShouldHaveValidationErrorFor(m => m.Description)
            .WithErrorMessage("Description must only contain letters, digits, spaces, and common punctuation.")
            .Only();
    }
}