using FluentValidation.TestHelper;
using NettoRetail.API;
using NettoRetail.API.Infrastructure.Validators;
using NettoRetail.Application.Models.Order;
using NettoRetail.Shared.Constants;

namespace NettoRetail.Tests.UnitTests.SearchTests;

public class SearchFieldModelValidatorTests
{
    private readonly SearchFieldModelValidator _validator;

    public SearchFieldModelValidatorTests()
    {
        _validator = new SearchFieldModelValidator();
    }

    [Fact]
    public void Should_Have_Error_When_Field_Is_Invalid()
    {
        var model = new SearchFieldModel { Field = "InvalidField", SearchTerm = "test" };

        var result = _validator.TestValidate(model);

        result.ShouldHaveValidationErrorFor(x => x.Field)
            .WithErrorMessage(ValidationErrorMessages.Search.InvalidSearchField);

        Assert.True(result.Errors.Count == 1);
    }
    [Fact]
    public void Should_Have_Error_When_SearchTerm_Is_Empty()
    {
        var model = new SearchFieldModel { Field = "customerfirstname", SearchTerm = string.Empty };

        var result = _validator.TestValidate(model);

        result.ShouldHaveValidationErrorFor(x => x.SearchTerm)
            .WithErrorMessage(ValidationErrorMessages.Search.SearchTermRequired);

        Assert.True(result.Errors.Count == 1);
    }
    [Fact]
    public void Should_Have_Error_When_SearchTerm_Exceeds_MaxLength()
    {
        var model = new SearchFieldModel { Field = "customerfirstname", SearchTerm = new string('a', 101) };

        var result = _validator.TestValidate(model);

        result.ShouldHaveValidationErrorFor(x => x.SearchTerm)
            .WithErrorMessage(ValidationErrorMessages.Search.SearchTermMaxLength);

        Assert.True(result.Errors.Count == 1);
    }
    [Theory]
    [InlineData("orderid", "test")]
    [InlineData("customerfirstname", "1234++")]
    [InlineData("customeremail", "something")]
    public void Should_Have_Error_When_SearchTerm_Has_Invalid_Characters_For_Field(string field, string searchTerm)
    {
        var model = new SearchFieldModel { Field = field, SearchTerm = searchTerm };

        var result = _validator.TestValidate(model);

        var failures = result.ShouldHaveValidationErrorFor(x => x.SearchTerm)
            .Where(x => x.ErrorMessage == ValidationErrorMessages.Search.InvalidCharacters
            || x.ErrorMessage == ValidationErrorMessages.Search.InvalidNumericValue
            || x.ErrorMessage == ValidationErrorMessages.Search.InvalidEmailFormat);

        Assert.NotEmpty(failures);
    }
    [Theory]
    [InlineData("orderid", "1234")]
    [InlineData("customerfirstname", "george")]
    [InlineData("customeremail", "george@gmail.com")]
    public void Should_Succeed_When_SearchTerm_Has_Valid_Characters_For_Field(string field, string searchTerm)
    {
        var model = new SearchFieldModel { Field = field, SearchTerm = searchTerm };

        var result = _validator.TestValidate(model);

        result.ShouldNotHaveValidationErrorFor(x => x.SearchTerm);
    }
}
