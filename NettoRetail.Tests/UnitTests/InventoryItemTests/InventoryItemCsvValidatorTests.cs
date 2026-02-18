using NettoRetail.API.Infrastructure.Validators;
using NettoRetail.Application.Exceptions;
using NettoRetail.Application.Models.CsvFileModel;
using NettoRetail.Domain.Enums;

namespace NettoRetail.Tests.UnitTests.InventoryItemTests;

public class InventoryItemCsvValidatorTests
{
    private static InventoryItemCsvModel CreateValidModel()
        => new()
        {
            ProductUpc = 123456789012,
            ProductName = "Valid_Name-123",
            ProductCategory = "Electronics & Gadgets",
            ProductLocation = "A-01",
            Brand = "BrandX",
            AvailableAmount = 10,
            ReservedAmount = 2,
            OnHandAmount = 15,
            Price = 12.50m,
            Status = InventoryItemStatusEnum.Available,
            Description = "Simple description."
        };

    [Fact]
    public void Validator_Should_Fail_When_ProductName_HasSpaces()
    {
        var validator = new InventoryItemCsvValidator();
        var model = CreateValidModel();
        model.ProductName = "Invalid Name";

        var result = validator.Validate(model);

        Assert.False(result.IsValid);
    }

    [Fact]
    public void Validator_Should_Fail_When_Upc_IsNot12Digits()
    {
        var validator = new InventoryItemCsvValidator();
        var model = CreateValidModel();
        model.ProductUpc = 12345;

        var result = validator.Validate(model);

        Assert.False(result.IsValid);
    }

    [Fact]
    public void Validator_Should_Fail_When_Category_HasLeadingSpace()
    {
        var validator = new InventoryItemCsvValidator();
        var model = CreateValidModel();
        model.ProductCategory = " Electronics & Gadgets";

        var result = validator.Validate(model);

        Assert.False(result.IsValid);
    }

    [Fact]
    public async Task ListValidator_Should_Fail_On_DuplicateUpc()
    {
        var rowValidator = new InventoryItemCsvValidator();
        var listValidator = new InventoryItemsCsvListValidator(rowValidator);

        var first = CreateValidModel();
        var second = CreateValidModel();
        second.ProductName = "AnotherName";

        var items = new List<InventoryItemCsvModel> { first, second };

        await Assert.ThrowsAsync<BadRequestException>(() => listValidator.ValidateAsync(items));
    }
}
