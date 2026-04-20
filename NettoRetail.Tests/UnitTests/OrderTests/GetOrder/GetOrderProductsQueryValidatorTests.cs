using FluentValidation.TestHelper;
using NettoRetail.API.Infrastructure.Validators;
using NettoRetail.Application.CQRS.Orders.Queries;

namespace NettoRetail.Tests.UnitTests.OrderTests.GetOrder;

public class GetOrderProductsQueryValidatorTests
{
    private readonly GetOrderProductsQueryValidator _validator;
    public GetOrderProductsQueryValidatorTests()
    {
        _validator = new GetOrderProductsQueryValidator();
    }

    [Fact]
    public void Validate_ShouldPass_WhenInputIsCorrect()
    {
        var query = new GetOrderProductsQuery(23, 45);
        var result = _validator.TestValidate(query);

        result.ShouldNotHaveAnyValidationErrors();
    }

    [Fact]
    public void Validate_ShouldFail_WhenOrderIdIsLessThanZero()
    {
        var query = new GetOrderProductsQuery(-23, 23);

        var result = _validator.TestValidate(query);

        result.ShouldHaveValidationErrorFor(x => x.OrderId);
    }

    [Fact]
    public void Validate_ShouldFail_WhenShopIdIsLessThanZero()
    {
        var query = new GetOrderProductsQuery(23, -23);

        var result = _validator.TestValidate(query);

        result.ShouldHaveValidationErrorFor(x => x.UserShopId);
    }
}
