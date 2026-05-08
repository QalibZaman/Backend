using FluentValidation.TestHelper;
using NettoRetail.API.Infrastructure.Validators;
using NettoRetail.Application.CQRS.Orders.Queries;

namespace NettoRetail.Tests.UnitTests.OrderTests.GetOrderStatusHistory;

public class GetOrderStatusHistoryQueryValidatorTests
{
    private readonly GetOrderStatusHistoryQueryValidator _validator;
    public GetOrderStatusHistoryQueryValidatorTests()
    {
        _validator = new GetOrderStatusHistoryQueryValidator();
    }

    [Fact]
    public async Task Validator_ShouldReturnBadRequest_WhenShopIdIsLessThanZero()
    {
        var query = new GetOrderStatusHistoryQuery(2, -21);
        var result = _validator.TestValidate(query);

        result.ShouldHaveValidationErrorFor(x => x.ShopId);
    }

    [Fact]
    public async Task Validator_ShouldReturnBadRequest_WhenOrderIdIsLessThanZero()
    {
        var query = new GetOrderStatusHistoryQuery(-21, 2);
        var result = _validator.TestValidate(query);

        result.ShouldHaveValidationErrorFor(x => x.OrderId);
    }

    [Fact]
    public async Task Validator_ShouldPass_WhenAllTheDataIsCorrect()
    {
        var query = new GetOrderStatusHistoryQuery(2, 2);
        var result = _validator.TestValidate(query);

        result.ShouldNotHaveAnyValidationErrors();
    }
}
