using FluentValidation.TestHelper;
using NettoRetail.API;
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
    public void Should_Have_Error_When_Content_Is_Empty()
    {
        var model = new UpdateContentRequest { Content = string.Empty, IsVisible = true};
        var result = _validator.TestValidate(model);

        result.ShouldHaveValidationErrorFor(x => x.Content)
            .WithErrorMessage(ValidationErrorMessages.Content.IsRequired);

        Assert.True(result.Errors.Count == 2);
    }
    [Fact]
    public void Should_Have_Error_When_Content_Exceeds_Max_Length()
    {
        var MaxContentLength = 50 * 1024;
        var model = new UpdateContentRequest { Content = new string('a', MaxContentLength + 1), IsVisible = true };
        
        var result = _validator.TestValidate(model);

        result.ShouldHaveValidationErrorFor(x => x.Content)
            .WithErrorMessage($"Content cannot exceed {MaxContentLength} characters.");
        Assert.True(result.Errors.Count == 1);
    }
    [Fact]
    public void Should_Have_Error_When_Content_Is_Whitespace_But_IsVisible_Is_True()
    {
        var model = new UpdateContentRequest { Content = " ", IsVisible = true };

        var result = _validator.TestValidate(model);

        result.ShouldHaveValidationErrorFor(x => x.IsVisible)
            .WithErrorMessage(ValidationErrorMessages.Content.ContentVisibilityRequiredMessage);
        Assert.True(result.Errors.Count == 2);
    }
}
