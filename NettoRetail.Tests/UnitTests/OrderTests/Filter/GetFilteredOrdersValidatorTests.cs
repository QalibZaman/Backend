using FluentAssertions;
using FluentValidation.TestHelper;
using NettoRetail.API.Infrastructure.Validators;
using NettoRetail.Application.Models.Order;
using NettoRetail.Shared.Constants;

namespace NettoRetail.Tests.UnitTests.OrderTests.Filter;

public class GetFilteredOrdersValidatorTests
{
    private readonly GetFilteredOrdersValidator _validator = new();

    [Fact]
    public void Should_Allow_Partial_Email_Filter_Value()
    {
        var model = new GetFilteredOrdersModel
        {
            CustomerEmails = ["test@gmail.com"]
        };

        var result = _validator.TestValidate(model);

        result.Errors.Should().NotContain(e => e.PropertyName == "CustomerEmails[0]");
    }

    [Fact]
    public void Should_Allow_Common_Punctuation_In_Creator_Filter()
    {
        var model = new GetFilteredOrdersModel
        {
            Creators = ["J.Doe-2"]
        };

        var result = _validator.TestValidate(model);

        result.Errors.Should().NotContain(e => e.PropertyName == "Creators[0]");
    }

    [Fact]
    public void Should_Have_Error_When_Email_Filter_Has_Invalid_Characters()
    {
        var model = new GetFilteredOrdersModel
        {
            CustomerEmails = ["abc<def"]
        };

        var result = _validator.TestValidate(model);

        result.Errors.Should().ContainSingle(e =>
            e.PropertyName == "CustomerEmails[0]" &&
            e.ErrorMessage == ValidationErrorMessages.Common.InvalidEmailFormatErrorMessage);
    }
}
