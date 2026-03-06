using FluentValidation.TestHelper;
using NettoRetail.API.Infrastructure.Validators;
using NettoRetail.Application.Models.InventoryItem;
using NettoRetail.Domain.Enums;
using NettoRetail.Shared.Constants;

namespace NettoRetail.Tests.UnitTests.UpdateProductValidatorTests;

public class DetailValidatorTests
{
    private readonly DetailValidator _validator;
    public DetailValidatorTests()
    {
        _validator = new DetailValidator();
    }

    [Fact]
    public void PassingEmptyModel_ShouldBeInvalid()
    {
        var model = new DetailModel();

        var result = _validator.TestValidate(model);

        result.ShouldHaveValidationErrorFor(m => m)
            .WithErrorMessage(ValidationErrorMessages.ANY_PROP_UPDATE_ERROR_MESSAGE)
            .Only();
    }

    [Fact]
    public void PassingDeleteCommandWithoutId_ShouldBeInvalid()
    {
        var model = new DetailModel()
        {
            IsDelete = true
        };

        var result = _validator.TestValidate(model);

        result.ShouldHaveAnyValidationError();
        Assert.Equal(2, result.Errors.Count);
    }

    [Fact]
    public void PassingDeleteCommandWithId_ShouldBeValid()
    {
        var model = new DetailModel()
        {
            Id = Guid.NewGuid(),
            IsDelete = true
        };

        var result = _validator.TestValidate(model);

        result.ShouldNotHaveAnyValidationErrors();
    }

    [Fact]
    public void PassingNumberWithoutUnit_ShouldBeInvalid()
    {
        var model = new DetailModel()
        {
            TypeEnum = ProductDetailTypeEnum.Number
        };

        var result = _validator.TestValidate(model);

        result.ShouldHaveValidationErrorFor(m => m.Unit);
        Assert.Equal(4, result.Errors.Count);
    }

    [Fact]
    public void PassingNumberWithUnitButWithoutValue_ShouldBeInvalid()
    {
        var model = new DetailModel()
        {
            TypeEnum = ProductDetailTypeEnum.Number,
            Unit = ProductDetailUnitEnum.cm
        };

        var result = _validator.TestValidate(model);

        result.ShouldHaveValidationErrorFor(m => m.Value);
        Assert.Equal(3, result.Errors.Count);
    }

    [Fact]
    public void PassingEmptyName_ShouldBeInvalid()
    {
        var model = new DetailModel()
        {
            Name = ""
        };

        var result = _validator.TestValidate(model);

        result.ShouldHaveAnyValidationError();
        Assert.Equal(2, result.Errors.Count);
    }

    [Fact]
    public void PassingNameWithIncorrectChars_ShouldBeInvalid()
    {
        var model = new DetailModel()
        {
            Id = Guid.NewGuid(),
            Name = "...*Name"
        };

        var result = _validator.TestValidate(model);

        result.ShouldHaveValidationErrorFor(m => m.Name)
            .WithErrorMessage("Invalid format for the name.")
            .Only();
    }

    [Fact]
    public void PassingLongName_ShouldBeInvalid()
    {
        var model = new DetailModel()
        {
            Id = Guid.NewGuid(),
            Name = "Loremipsumdolorsitamet,consectetueradipiscingadipiscing"
        };

        var result = _validator.TestValidate(model);

        result.ShouldHaveValidationErrorFor(m => m.Name)
            .WithErrorMessage("Invalid format for the name.")
            .Only();
    }

    [Fact]
    public void PassingNumberWithUnitButWithIncorrectValue_ShouldBeInvalid()
    {
        var model = new DetailModel()
        {
            TypeEnum = ProductDetailTypeEnum.Number,
            Unit = ProductDetailUnitEnum.cm,
            Value = "f"
        };

        var result = _validator.TestValidate(model);

        result.ShouldHaveAnyValidationError();
        Assert.Equal(2, result.Errors.Count);
    }

    [Fact]
    public void PassingDetailForAddition_ShouldBeValid()
    {
        var model = new DetailModel()
        {
            Name = "NewDetail",
            TypeEnum = ProductDetailTypeEnum.Number,
            Unit = ProductDetailUnitEnum.cm,
            Value = "2"
        };

        var result = _validator.TestValidate(model);

        result.ShouldNotHaveAnyValidationErrors();
    }

    [Fact]
    public void PassingColorWithUnit_ShouldBeInvalid()
    {
        var model = new DetailModel()
        {
            TypeEnum = ProductDetailTypeEnum.Color,
            Unit = ProductDetailUnitEnum.cm
        };

        var result = _validator.TestValidate(model);

        result.ShouldHaveAnyValidationError();
        Assert.Equal(3, result.Errors.Count);
    }

    [Fact]
    public void PassingColorWithIncorrectValue_ShouldBeInvalid()
    {
        var model = new DetailModel()
        {
            TypeEnum = ProductDetailTypeEnum.Color,
            Value = "123"
        };

        var result = _validator.TestValidate(model);

        result.ShouldHaveAnyValidationError();
        Assert.Equal(2, result.Errors.Count);
    }

    [Fact]
    public void PassingColorDetailForAddition_ShouldBeValid()
    {
        var model = new DetailModel()
        {
            Name = "ColorDetail",
            TypeEnum = ProductDetailTypeEnum.Color,
            Value = "#fffaaa"
        };

        var result = _validator.TestValidate(model);

        result.ShouldNotHaveAnyValidationErrors();
    }

    [Fact]
    public void PassingLogicWithUnit_ShouldBeInvalid()
    {
        var model = new DetailModel()
        {
            Name = "LogicDetail",
            TypeEnum = ProductDetailTypeEnum.Logic,
            Unit = ProductDetailUnitEnum.cm,
            Value = "true"
        };

        var result = _validator.TestValidate(model);

        result.ShouldHaveAnyValidationError();
        Assert.NotEmpty(result.Errors);
    }

    [Fact]
    public void PassingLogicWithIncorrectValue_ShouldBeInvalid()
    {
        var model = new DetailModel()
        {
            Name = "IncorrectLogic",
            TypeEnum = ProductDetailTypeEnum.Logic,
            Value = "sadsad"
        };

        var result = _validator.TestValidate(model);

        result.ShouldHaveValidationErrorFor(m => m.Value)
            .WithErrorMessage("Value should be a valid boolean.")
            .Only();
    }

    [Fact]
    public void PassingLogicWithCorrectValue_ShouldBeValid()
    {
        var model = new DetailModel()
        {
            Name = "LogicDetail",
            TypeEnum = ProductDetailTypeEnum.Logic,
            Value = "true"
        };

        var result = _validator.TestValidate(model);

        result.ShouldNotHaveAnyValidationErrors();
    }

    [Fact]
    public void PassingStringWithUnit_ShouldBeInvalid()
    {
        var model = new DetailModel()
        {
            TypeEnum = ProductDetailTypeEnum.String,
            Unit = ProductDetailUnitEnum.cm
        };

        var result = _validator.TestValidate(model);

        result.ShouldHaveAnyValidationError();
        Assert.Equal(3, result.Errors.Count);
    }

    [Fact]
    public void PassingStringWithIncorrectValue_ShouldBeInvalid()
    {
        var model = new DetailModel()
        {
            Name = "IncorrectStringDetail",
            TypeEnum = ProductDetailTypeEnum.String,
            Value = "!"
        };

        var result = _validator.TestValidate(model);

        result.ShouldHaveValidationErrorFor(m => m.Value)
            .WithErrorMessage("Invalid format for the value.")
            .Only();
    }

    [Fact]
    public void PassingStringDetail_ShouldBeValid()
    {
        var model = new DetailModel()
        {
            Name = "StringDetail",
            TypeEnum = ProductDetailTypeEnum.String,
            Value = "Hello World"
        };

        var result = _validator.TestValidate(model);

        result.ShouldNotHaveAnyValidationErrors();
    }

    [Fact]
    public void PassingUnitAndValueWithoutType_ShouldBeInvalid()
    {
        var model = new DetailModel()
        {
            Unit = ProductDetailUnitEnum.kg,
            Value = "1"
        };

        var result = _validator.TestValidate(model);

        result.ShouldHaveAnyValidationError();
        Assert.Equal(4, result.Errors.Count);
    }
}