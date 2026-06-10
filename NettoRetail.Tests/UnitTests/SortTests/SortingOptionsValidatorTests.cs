using FluentValidation.TestHelper;
using NettoRetail.API.Infrastructure.Validators;
using NettoRetail.Application.Models.InventoryItem;
using NettoRetail.Application.Models.Request;
using NettoRetail.Shared.Constants;

namespace NettoRetail.Tests.UnitTests.SortTests;

public class SortingOptionsValidatorTests
{
    private readonly SortingOptionValidator _validator;
    public SortingOptionsValidatorTests()
    {
        _validator = new SortingOptionValidator();
    }

    [Fact]
    public void Validate_ShouldFail_WhenSortingOptionsFieldIsEmpty()
    {
        var model = new SortingOption
        {
            Field = string.Empty
        };
        var result = _validator.TestValidate(model);

        result.ShouldHaveValidationErrorFor(x => x.Field)
            .WithErrorMessage(ValidationErrorMessages.Sort.InvalidSortFieldErrorMessage);
    }
}
