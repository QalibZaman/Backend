using FluentValidation.TestHelper;
using NettoRetail.API.Infrastructure.Validators;
using NettoRetail.Application.CQRS.InventoryItems.Queries;
using NettoRetail.Application.Exceptions;
using NettoRetail.DataAccess.Specifications.Inventory;
using NettoRetail.Shared.Constants;
using NettoRetail.Shared.Interfaces.Repositories;
using NSubstitute;

namespace NettoRetail.Tests.UnitTests.InventoryItemTests;

public class GetInventoryItemDetailsQueryValidatorTests
{
    private readonly GetInventoryItemDetailsQueryValidator _validator;
    private readonly IInventoryItemRepository _inventoryItemRepository;
    public GetInventoryItemDetailsQueryValidatorTests()
    {
        _inventoryItemRepository = Substitute.For<IInventoryItemRepository>();
        _validator = new GetInventoryItemDetailsQueryValidator(_inventoryItemRepository);
    }

    [Fact]
    public async Task Validate_ShouldPass_WhenItemExists()
    {
        var query = new GetInventoryItemDetailsQuery(Guid.NewGuid());

        _inventoryItemRepository.ExistsAsync(Arg.Any<GetItemByIdSpecification>(), Arg.Any<CancellationToken>()).Returns(true);

        var result = await _validator.TestValidateAsync(query);

        Assert.True(result.IsValid);

        result.ShouldNotHaveAnyValidationErrors();
    }

    [Fact]
    public async Task Validate_ShouldFail_WhenItemDoesntExist()
    {
        var query = new GetInventoryItemDetailsQuery(Guid.NewGuid());

        _inventoryItemRepository.ExistsAsync(Arg.Any<GetItemByIdSpecification>(), Arg.Any<CancellationToken>())
            .Returns(false);

        await Assert.ThrowsAsync<NotFoundException>(() => _validator.TestValidateAsync(query));
    }
}
