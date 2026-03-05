using FluentValidation.TestHelper;
using Microsoft.AspNetCore.Http;
using NettoRetail.API.Infrastructure.Validators;
using NettoRetail.Application.Models.InventoryItem;
using NettoRetail.Shared.Constants;

namespace NettoRetail.Tests.UnitTests.UpdateProductValidatorTests;

public class ImageValidatorTests
{
    private readonly ImageValidator _validator;
    public ImageValidatorTests()
    {
        _validator = new ImageValidator();
    }

    [Fact]
    public void PassingEmptyModel_ShouldBeInvalid()
    {
        var model = new ImageModel();

        var result = _validator.TestValidate(model);

        result.ShouldHaveValidationErrorFor(m => m)
            .WithErrorMessage(ValidationErrorMessages.ANY_PROP_UPDATE_ERROR_MESSAGE)
            .Only();
    }

    [Fact]
    public void PassingIncorrectFileFormat_ShouldBeInvalid()
    {
        var bytes = new byte[3 * 1024 * 1024];
        using var stream = new MemoryStream(bytes);
        var model = new ImageModel()
        {
            Image = new FormFile(stream, 0, bytes.Length, "index", "index.html")
            {
                Headers = new HeaderDictionary(),
                ContentType = "text/html"
            }
        };

        var result = _validator.TestValidate(model);

        result.ShouldHaveValidationErrorFor(m => m.Image)
            .WithErrorMessage("Only these image types are allowed: image/jpeg, image/png.")
            .Only();
    }

    [Fact]
    public void PassingIncorrectFileSize_ShouldBeInvalid()
    {
        var bytes = new byte[1 * 2 * 1024];
        using var stream = new MemoryStream(bytes);
        var model = new ImageModel()
        {
            Image = new FormFile(stream, 0, bytes.Length, "image", "image.png")
            {
                Headers = new HeaderDictionary(),
                ContentType = "image/png"
            }
        };

        var result = _validator.TestValidate(model);

        result.ShouldHaveValidationErrorFor(m => m.Image)
            .WithErrorMessage("The size of the image must be between 1MB and 5MB.")
            .Only();
    }

    [Fact]
    public void PassingCorrectFile_ShouldBeValid()
    {
        var bytes = new byte[1 * 1024 * 1024];
        using var stream = new MemoryStream(bytes);
        var model = new ImageModel()
        {
            Image = new FormFile(stream, 0, bytes.Length, "image", "image.png")
            {
                Headers = new HeaderDictionary(),
                ContentType = "image/png"
            }
        };

        var result = _validator.TestValidate(model);

        result.ShouldNotHaveAnyValidationErrors();
    }

    [Fact]
    public void PassingIncorrectDescriptionLength_ShouldBeInvalid()
    {
        var model = new ImageModel()
        {
            Description = "Lorem ipsum dolor sit amet, consectetuer adipiscing elit. Aenean commodo ligula eget dolor. Aenean massa. Cum sociis natoque p"
        };

        var result = _validator.TestValidate(model);

        result.ShouldHaveValidationErrorFor(m => m.Description)
            .WithErrorMessage("Description must be 125 characters at max.")
            .Only();
    }

    [Fact]
    public void PassingCorrectDescriptionLength_ShouldBeValid()
    {
        var model = new ImageModel()
        {
            Description = "Lorem ipsum dolor sit amet, consectetuer adipiscing elit. Aenean commodo ligula eget dolor. Aenean massa."
        };

        var result = _validator.TestValidate(model);

        result.ShouldNotHaveAnyValidationErrors();
    }
}