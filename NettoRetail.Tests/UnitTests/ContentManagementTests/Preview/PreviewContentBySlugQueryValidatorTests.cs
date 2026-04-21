using FluentValidation.TestHelper;
using NettoRetail.Application.CQRS.Contents.Queries;
using NettoRetail.Application.Models.Request.Content;
using NettoRetail.Shared.Constants;

namespace NettoRetail.Tests.UnitTests.ContentManagementTests.Preview;

public class PreviewContentBySlugQueryValidatorTests
{
    private readonly PreviewContentBySlugQueryValidator _validator = new();

    [Fact]
    public void Validator_Should_Fail_When_Content_Has_No_Meaningful_Content()
    {
        var query = new PreviewContentBySlugQuery(
            "1-OurShop",
            new UpdateContentRequest
            {
                Content = "<p><br></p>",
                IsVisible = true,
            });

        var result = _validator.TestValidate(query);

        result.ShouldHaveValidationErrorFor("Model.Content")
            .WithErrorMessage(ValidationErrorMessages.Content.IsRequired);
    }

    [Fact]
    public void Validator_Should_Fail_When_Image_Count_Exceeds_Limit()
    {
        var images = string.Concat(Enumerable.Repeat("<img src=\"https://cdn.example.com/image.png\" />", 11));
        var query = new PreviewContentBySlugQuery(
            "1-OurShop",
            new UpdateContentRequest
            {
                Content = $"<div>{images}</div>",
                IsVisible = true,
            });

        var result = _validator.TestValidate(query);

        result.ShouldHaveValidationErrorFor("Model.Content")
            .WithErrorMessage(ValidationErrorMessages.Content.ImageLimitExceededMessage);
    }

    [Fact]
    public void Validator_Should_Pass_When_Content_Is_Valid()
    {
        var query = new PreviewContentBySlugQuery(
            "1-OurShop",
            new UpdateContentRequest
            {
                Content = "<p>Hello</p>",
                IsVisible = true,
            });

        var result = _validator.TestValidate(query);

        result.ShouldNotHaveAnyValidationErrors();
    }
}
