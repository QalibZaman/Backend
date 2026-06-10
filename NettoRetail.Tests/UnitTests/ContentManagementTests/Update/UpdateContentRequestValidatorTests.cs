using FluentValidation.TestHelper;
using NettoRetail.API.Infrastructure.Validators;
using NettoRetail.Application.Models.Request.Content;
using NettoRetail.Shared.Constants;

namespace NettoRetail.Tests.UnitTests.ContentManagementTests.Update;

public class UpdateContentRequestValidatorTests
{
    private readonly UpdateContentRequestValidator _validator;
    public UpdateContentRequestValidatorTests()
    {
        _validator = new UpdateContentRequestValidator();
    }

    [Fact]
    public void Validator_Should_Fail_When_Content_Is_Empty()
    {
        var model = new UpdateContentRequest { Content = string.Empty, IsVisible = true};
        var result = _validator.TestValidate(model);

        result.ShouldHaveValidationErrorFor(x => x.Content)
            .WithErrorMessage(ValidationErrorMessages.Content.IsRequired);
        Assert.Single(result.Errors);
    }
    [Fact]
    public void Validator_Should_Fail_When_Content_Exceeds_Max_Length()
    {
        var model = new UpdateContentRequest { Content = new string('a', ValidationErrorMessages.Content.MaxContentLength + 1), IsVisible = true };
        
        var result = _validator.TestValidate(model);

        result.ShouldHaveValidationErrorFor(x => x.Content)
            .WithErrorMessage($"Content cannot exceed {ValidationErrorMessages.Content.MaxContentLength} characters.");
        Assert.True(result.Errors.Count == 1);
    }
    [Fact]
    public void Validator_Should_Fail_When_Content_Is_Whitespace_But_IsVisible_Is_True()
    {
        var model = new UpdateContentRequest { Content = " ", IsVisible = true };

        var result = _validator.TestValidate(model);

        result.ShouldHaveValidationErrorFor(x => x.Content)
            .WithErrorMessage(ValidationErrorMessages.Content.IsRequired);
        Assert.Single(result.Errors);
    }

    [Fact]
    public void Validator_Should_Fail_When_Content_Has_Only_Empty_Html()
    {
        var model = new UpdateContentRequest { Content = "<p><br></p>", IsVisible = false };

        var result = _validator.TestValidate(model);

        result.ShouldHaveValidationErrorFor(x => x.Content)
            .WithErrorMessage(ValidationErrorMessages.Content.IsRequired);
    }

    [Fact]
    public void Validator_Should_Fail_When_Image_Count_Exceeds_Limit()
    {
        var images = string.Concat(Enumerable.Repeat("<img src=\"https://cdn.example.com/image.png\" />", 11));
        var model = new UpdateContentRequest { Content = $"<div>{images}</div>", IsVisible = true };

        var result = _validator.TestValidate(model);

        result.ShouldHaveValidationErrorFor(x => x.Content)
            .WithErrorMessage(ValidationErrorMessages.Content.ImageLimitExceededMessage);
    }
}
