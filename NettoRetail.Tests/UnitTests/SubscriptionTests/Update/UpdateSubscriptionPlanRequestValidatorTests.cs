using FluentValidation.TestHelper;
using NettoRetail.API.Infrastructure.Validators;
using NettoRetail.Shared.Constants;

namespace NettoRetail.Tests.UnitTests.SubscriptionTests.Update;

public class UpdateSubscriptionPlanRequestValidatorTests
{
    private readonly UpdateSubscriptionPlanRequestValidator _validator;
    public UpdateSubscriptionPlanRequestValidatorTests()
    {
        _validator = new UpdateSubscriptionPlanRequestValidator();
    }

    [Fact]
    public void Validate_ShouldPass_WhenUpdateSubscriptionPlanRequestIsValid()
    {
        //Arrange
        var model = new UpdateSubscriptionPlanRequest()
        {
            Name = "name",
            Description = "description",
            FeatureIds = new List<int> { 1, 2 },
            Motto = "motto",
            Price = 100
        };

        //Act
        var result = _validator.TestValidate(model);

        //Assert
        Assert.True(result.IsValid);
        Assert.True(result.Errors.Count == 0);
        result.ShouldNotHaveAnyValidationErrors();
    }

    [Fact]
    public void Validate_ShouldFail_WhenNameIsEmpty()
    {
        //Arrange
        var model = new UpdateSubscriptionPlanRequest()
        {
            Name = "",
            Description = "description",
            FeatureIds = new List<int> { 1, 2 },
            Motto = "motto",
            Price = 100
        };
        //Act
        var result = _validator.TestValidate(model);

        //Assert
        Assert.False(result.IsValid);
        Assert.True(result.Errors.Count == 1);
        result.ShouldHaveValidationErrorFor(x => x.Name);
    }

    [Fact]
    public void Validate_ShouldFail_WhenNameContainsInvalidCharacters()
    {
        //Arrange
        var model = new UpdateSubscriptionPlanRequest()
        {
            Name = "name@",
            Description = "description",
            FeatureIds = new List<int> { 1, 2 },
            Motto = "motto",
            Price = 100
        };

        //Act
        var result = _validator.TestValidate(model);

        //Assert
        Assert.False(result.IsValid);
        Assert.True(result.Errors.Count == 1);
        result.ShouldHaveValidationErrorFor(x => x.Name)
            .WithErrorMessage(ValidationErrorMessages.Subscription.NameFormatErrorMessage);
    }

    [Theory]
    [InlineData("q")]
    [InlineData("qwertyuiopasdfghjklzxcvbnm")]
    public void Validate_ShouldFail_WhenNameHasInvalidMinLengthMaxLength(string name)
    {
        //Arrange
        var model = new UpdateSubscriptionPlanRequest()
        {
            Name = name,
            Description = "description",
            FeatureIds = new List<int> { 1, 2 },
            Motto = "motto",
            Price = 100
        };

        //Act
        var result = _validator.TestValidate(model);

        //Assert
        Assert.False(result.IsValid);
        Assert.True(result.Errors.Count == 1);
        result.ShouldHaveValidationErrorFor(x => x.Name);
    }

    [Theory]
    [InlineData("Name--Invalid")]
    [InlineData("Name__Invalid")]
    [InlineData("Name-_Invalid")]
    [InlineData("Name_-Invalid")]
    public void Validate_ShouldFail_WhenNameHasConsecutiveSpecialChars(string name)
    {
        // Arrange
        var model = new UpdateSubscriptionPlanRequest()
        {
            Name = name,
            Description = "description",
            FeatureIds = new List<int> { 1, 2 },
            Motto = "motto",
            Price = 100
        };

        // Act
        var result = _validator.TestValidate(model);

        // Assert
        Assert.False(result.IsValid);
        Assert.True(result.Errors.Count == 1);
        result.ShouldHaveValidationErrorFor(x => x.Name)
              .WithErrorMessage(ValidationErrorMessages.Subscription.NameConsecutiveSpecialCharsErrorMessage);
    }

    [Fact]
    public void Validate_ShouldFail_WhenPriceIsNegative()
    {
        //Arrange
        var model = new UpdateSubscriptionPlanRequest()
        {
            Name = "name",
            Description = "description",
            FeatureIds = new List<int> { 1, 2 },
            Motto = "motto",
            Price = -10
        };

        //Act
        var result = _validator.TestValidate(model);

        //Assert
        Assert.False(result.IsValid);
        Assert.True(result.Errors.Count == 1);
        result.ShouldHaveValidationErrorFor(x => x.Price)
            .WithErrorMessage(ValidationErrorMessages.Subscription.PriceRequiredErrorMessage);
    }

    [Fact]
    public void Validate_ShouldFail_WhenDescriptionIsEmpty()
    {
        //Arrange
        var model = new UpdateSubscriptionPlanRequest()
        {
            Name = "name",
            Description = "",
            FeatureIds = new List<int> { 1, 2 },
            Motto = "motto",
            Price = 100
        };
        //Act
        var result = _validator.TestValidate(model);

        //Assert
        Assert.False(result.IsValid);
        Assert.True(result.Errors.Count == 1);
        result.ShouldHaveValidationErrorFor(x => x.Description);
    }

    [Fact]
    public void Validate_ShouldFail_WhenDescriptionExceedsMaxLength()
    {
        //Arrange
        var model = new UpdateSubscriptionPlanRequest()
        {
            Name = "name",
            Description = new string('a', 501),
            FeatureIds = new List<int> { 1, 2 },
            Motto = "motto",
            Price = 100
        };

        //Act
        var result = _validator.TestValidate(model);

        //Assert
        Assert.False(result.IsValid);
        Assert.True(result.Errors.Count == 1);
        result.ShouldHaveValidationErrorFor(x => x.Description);
    }

    [Fact]
    public void Validate_ShouldFail_WhenDescriptionContainsInvalidCharacters()
    {
        //Arrange
        var model = new UpdateSubscriptionPlanRequest()
        {
            Name = "name",
            Description = "description@",
            FeatureIds = new List<int> { 1, 2 },
            Motto = "motto",
            Price = 100
        };

        //Act
        var result = _validator.TestValidate(model);

        //Assert
        Assert.False(result.IsValid);
        Assert.True(result.Errors.Count == 1);
        result.ShouldHaveValidationErrorFor(x => x.Description)
            .WithErrorMessage(ValidationErrorMessages.Subscription.DescriptionFormatErrorMessage);
    }

    [Fact]
    public void Validate_ShouldFail_WhenFeatureIdsIsEmpty()
    {
        //Arrange
        var model = new UpdateSubscriptionPlanRequest()
        {
            Name = "name",
            Description = "description",
            FeatureIds = new List<int>(),
            Motto = "motto",
            Price = 100
        };

        //Act
        var result = _validator.TestValidate(model);

        //Assert
        Assert.False(result.IsValid);
        Assert.True(result.Errors.Count == 1);
        result.ShouldHaveValidationErrorFor(x => x.FeatureIds);
    }

    [Fact]
    public void Validate_ShouldFail_WhenMottoIsEmpty()
    {
        //Arrange
        var model = new UpdateSubscriptionPlanRequest()
        {
            Name = "name",
            Description = "description",
            FeatureIds = new List<int> { 1, 2 },
            Motto = "",
            Price = 100
        };

        //Act
        var result = _validator.TestValidate(model);

        //Assert
        Assert.False(result.IsValid);
        Assert.True(result.Errors.Count == 1);
        result.ShouldHaveValidationErrorFor(x => x.Motto);
    }

    [Fact]
    public void Validate_ShouldFail_WhenMottoExceedsMaxLength()
    {
        //Arrange
        var model = new UpdateSubscriptionPlanRequest()
        {
            Name = "name",
            Description = "description",
            FeatureIds = new List<int> { 1, 2 },
            Motto = new string('a', 71),
            Price = 100
        };

        //Act
        var result = _validator.TestValidate(model);

        //Assert
        Assert.False(result.IsValid);
        Assert.True(result.Errors.Count == 1);
        result.ShouldHaveValidationErrorFor(x => x.Motto);
    }

    [Fact]
    public void Validate_ShouldFail_WhenMottoContainsInvalidCharacters()
    {
        //Arrange
        var model = new UpdateSubscriptionPlanRequest()
        {
            Name = "name",
            Description = "description",
            FeatureIds = new List<int> { 1, 2 },
            Motto = "motto@",
            Price = 100
        };

        //Act
        var result = _validator.TestValidate(model);

        //Assert
        Assert.False(result.IsValid);
        Assert.True(result.Errors.Count == 1);
        result.ShouldHaveValidationErrorFor(x => x.Motto)
            .WithErrorMessage(ValidationErrorMessages.Subscription.MottoFormatErrorMessage);
    }

    [Fact]
    public void Validate_ShouldFail_WhenFeatureIdsContainsDuplicateValues()
    {
        //Arrange
        var model = new UpdateSubscriptionPlanRequest()
        {
            Name = "name",
            Description = "description",
            FeatureIds = new List<int> { 1, 1 },
            Motto = "motto",
            Price = 100
        };

        //Act
        var result = _validator.TestValidate(model);

        //Assert
        Assert.False(result.IsValid);
        Assert.True(result.Errors.Count == 1);
        result.ShouldHaveValidationErrorFor(x => x.FeatureIds)
            .WithErrorMessage(ValidationErrorMessages.Subscription.FeatureIdsUniqueErrorMessage);
    }

    [Fact]
    public void Validate_ShouldFail_WhenFeatureIdsIsNull()
    {
        //Arrange
        var model = new UpdateSubscriptionPlanRequest()
        {
            Name = "name",
            Description = "description",
            Motto = "motto",
            Price = 100
        };

        //Act
        var result = _validator.TestValidate(model);

        //Assert
        Assert.False(result.IsValid);
        Assert.True(result.Errors.Count == 1);
        result.ShouldHaveValidationErrorFor(x => x.FeatureIds)
            .WithErrorMessage(ValidationErrorMessages.Subscription.FeatureIdsRequiredErrorMessage);
    }

    [Fact]
    public void Validate_ShouldFail_WhenFeatureIdsContainsNonPositiveValues()
    {
        //Arrange
        var model = new UpdateSubscriptionPlanRequest()
        {
            Name = "name",
            Description = "description",
            FeatureIds = new List<int> { 1, -1 },
            Motto = "motto",
            Price = 100
        };
        //Act
        var result = _validator.TestValidate(model);
        //Assert
        Assert.False(result.IsValid);
        Assert.True(result.Errors.Count == 1);
        result.ShouldHaveValidationErrorFor(x => x.FeatureIds)
            .WithErrorMessage(ValidationErrorMessages.Subscription.FeatureIdsPositiveErrorMessage);
    }
}
