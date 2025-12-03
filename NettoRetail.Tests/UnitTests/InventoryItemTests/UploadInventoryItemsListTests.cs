using MediatR;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using NettoRetail.API.Controllers.V1;
using NettoRetail.API.Infrastructure.Validators;
using NettoRetail.Application.CQRS.InventoryItems.Commands;
using NettoRetail.Application.Exceptions;
using NettoRetail.Application.Models.InventoryItem;
using NettoRetail.Shared.Constants;
using NSubstitute;
using System.Security.Claims;


namespace NettoRetail.Tests.UnitTests.InventoryItemTests;

public class UploadInventoryItemsListTests
{
    private readonly IMediator _mediatorMock;
    private readonly InventoryItemsController _controller;
    private readonly HttpContext _httpContextMock;
    private readonly ClaimsPrincipal _userMock;

    public UploadInventoryItemsListTests()
    {
        _mediatorMock = Substitute.For<IMediator>();
        _controller = new InventoryItemsController(_mediatorMock);
        _httpContextMock = Substitute.For<HttpContext>();
        _userMock = Substitute.For<ClaimsPrincipal>();
    }

    private void SetupHttpContext(int shopId)
    {
        var shopIdClaim = new Claim(AuthClaims.ShopId, shopId.ToString());
        _userMock.FindFirst(AuthClaims.ShopId).Returns(shopIdClaim);
        _httpContextMock.User.Returns(_userMock);
        _controller.ControllerContext = new ControllerContext { HttpContext = _httpContextMock };
    }

    [Fact]
    public async Task UploadInventoryItemsList_ShouldReturnOk_WithValidCsvFile()
    {
        // Arrange
        const int shopId = 1;
        var cancellationToken = CancellationToken.None;

        SetupHttpContext(shopId);

        var csvFile = CreateMockFormFile("test.csv", "text/csv", "ProductName,Upc\nProduct1,123");

        _mediatorMock
            .Send(Arg.Any<UploadInventoryItemsListCommand>(), Arg.Any<CancellationToken>())
            .Returns(Task.CompletedTask);

        // Act
        var result = await _controller.UploadInventoryItemsList(csvFile, cancellationToken);

        // Assert
        var okResult = Assert.IsType<OkResult>(result);
        Assert.Equal(200, okResult.StatusCode);

        await _mediatorMock.Received(1).Send(
            Arg.Is<UploadInventoryItemsListCommand>(cmd =>
                cmd.CsvItemsFile == csvFile && cmd.UserShopId == shopId),
            cancellationToken);
    }


    [Fact]
    public async Task UploadInventoryItemsList_ShouldHandleNullFile()
    {
        // Arrange
        const int shopId = 1;
        var cancellationToken = CancellationToken.None;

        SetupHttpContext(shopId);

        _mediatorMock
            .Send(Arg.Any<UploadInventoryItemsListCommand>(), Arg.Any<CancellationToken>())
            .Returns(Task.CompletedTask);

        // Act
        var result = await _controller.UploadInventoryItemsList(null, cancellationToken);

        // Assert
        var okResult = Assert.IsType<OkResult>(result);

        await _mediatorMock.Received(1).Send(
            Arg.Is<UploadInventoryItemsListCommand>(cmd => cmd.CsvItemsFile == null && cmd.UserShopId == shopId),
            cancellationToken);
    }

    [Fact]
    public async Task UploadInventoryItemsList_ShouldPropagateBadRequestException_FromHandler()
    {
        // Arrange
        const int shopId = 1;
        var cancellationToken = CancellationToken.None;

        SetupHttpContext(shopId);

        var csvFile = CreateMockFormFile("test.csv", "text/csv", "test");

        _mediatorMock
            .Send(Arg.Any<UploadInventoryItemsListCommand>(), Arg.Any<CancellationToken>())
            .Returns(Task.FromException(new BadRequestException("CSV file is empty.")));

        // Act & Assert
        await Assert.ThrowsAsync<BadRequestException>(() => _controller.UploadInventoryItemsList(csvFile, cancellationToken));
    }

    [Fact]
    public async Task UploadInventoryItemsList_ShouldPropagateConflictException_FromHandler()
    {
        // Arrange
        const int shopId = 1;
        var cancellationToken = CancellationToken.None;

        SetupHttpContext(shopId);

        var csvFile = CreateMockFormFile("test.csv", "text/csv", "test");

        _mediatorMock
            .Send(Arg.Any<UploadInventoryItemsListCommand>(), Arg.Any<CancellationToken>())
            .Returns(Task.FromException(new ConflictException("Item with such UPC is registered.")));

        // Act & Assert
        await Assert.ThrowsAsync<ConflictException>(() => _controller.UploadInventoryItemsList(csvFile, cancellationToken));
    }

    [Fact]
    public async Task UploadInventoryItemsList_ShouldPropagateNotFoundException_FromHandler()
    {
        // Arrange
        const int shopId = 1;
        var cancellationToken = CancellationToken.None;

        SetupHttpContext(shopId);

        var csvFile = CreateMockFormFile("test.csv", "text/csv", "test");

        _mediatorMock
            .Send(Arg.Any<UploadInventoryItemsListCommand>(), Arg.Any<CancellationToken>())
            .Returns(Task.FromException(new NotFoundException("There is no category with the such name.")));

        // Act & Assert
        await Assert.ThrowsAsync<NotFoundException>(() => _controller.UploadInventoryItemsList(csvFile, cancellationToken));
    }

    [Fact]
    public void Validator_Should_Fail_When_NoFieldsProvided()
    {
        var validator = new UpdateInventoryItemValidator();
        var model = new UpdateInventoryItemModel();

        var result = validator.Validate(model);

        Assert.False(result.IsValid);
        Assert.Contains(result.Errors, e => e.ErrorMessage.Contains("At least one field must be provided"));
    }

    [Fact]
    public void Validator_Should_Fail_When_ReservedGreaterThanAvailable()
    {
        var validator = new UpdateInventoryItemValidator();
        var model = new UpdateInventoryItemModel
        {
            AvailableAmount = 5,
            ReservedAmount = 10
        };

        var result = validator.Validate(model);

        Assert.False(result.IsValid);
        Assert.Contains(result.Errors, e => e.ErrorMessage.Contains("Reserved amount cannot exceed the available amount."));
    }

    [Fact]
    public void Validator_Should_Fail_When_OnHandLessThanReservedPlusAvailable()
    {
        var validator = new UpdateInventoryItemValidator();
        var model = new UpdateInventoryItemModel
        {
            AvailableAmount = 5,
            ReservedAmount = 3,
            OnHandAmount = 7 // should be >= 8
        };

        var result = validator.Validate(model);

        Assert.False(result.IsValid);
        Assert.Contains(result.Errors, e => e.ErrorMessage.Contains("The sum of Reserved Amount and Available Amount should not exceed the On-hand Amount."));
    }

    [Fact]
    public void Validator_Should_Pass_When_ProductNameIsValid()
    {
        var validator = new UpdateInventoryItemValidator();
        var model = new UpdateInventoryItemModel { ProductName = "Valid_Name-123" };

        var result = validator.Validate(model);

        Assert.True(result.IsValid);
    }

    [Fact]
    public void Validator_Should_Fail_When_ProductNameContainsInvalidCharacters()
    {
        var validator = new UpdateInventoryItemValidator();
        var model = new UpdateInventoryItemModel { ProductName = "Invalid Name With Spaces" };

        var result = validator.Validate(model);

        Assert.False(result.IsValid);
        Assert.Contains(result.Errors, e => e.ErrorMessage.Contains("Product name must only contain letters, digits, '-' and '_'"));
    }

    [Fact]
    public void Validator_Should_Fail_When_CategoryIdIsNotGreaterThanZero()
    {
        var validator = new UpdateInventoryItemValidator();
        var model = new UpdateInventoryItemModel { CategoryId = 0 };

        var result = validator.Validate(model);

        Assert.False(result.IsValid);
        Assert.Contains(result.Errors, e => e.PropertyName == "CategoryId" && e.ErrorMessage.Contains("Category id must be greater than 0."));
    }

    [Fact]
    public void Validator_Should_Fail_When_ProductUpcIsNot12Digits()
    {
        var validator = new UpdateInventoryItemValidator();
        var model = new UpdateInventoryItemModel { ProductUpc = 12345 };

        var result = validator.Validate(model);

        Assert.False(result.IsValid);
        Assert.Contains(result.Errors, e => e.PropertyName == "ProductUpc" && e.ErrorMessage.Contains("Product UPC must be exactly 12 digits long."));
    }

    [Fact]
    public void Validator_Should_Allow_Description_WithLeadingOrTrailingSpaces()
    {
        var validator = new UpdateInventoryItemValidator();
        var model = new UpdateInventoryItemModel { Description = " leading space" };

        var result = validator.Validate(model);

        Assert.True(result.IsValid);
        Assert.DoesNotContain(result.Errors, e => e.PropertyName == "Description" && e.ErrorMessage.Contains("Description must not have leading or trailing spaces."));
    }


    [Fact]
    public void Validator_Should_Fail_When_PromotionPriceIsNotLessThanPrice()
    {
        var validator = new UpdateInventoryItemValidator();
        var model = new UpdateInventoryItemModel { Price = 10.00m, PromotionPrice = 15.00m };

        var result = validator.Validate(model);

        Assert.False(result.IsValid);
        Assert.Contains(result.Errors, e => e.ErrorMessage.Contains("Promotion price must be less than the regular price."));
    }

    [Fact]
    public void Validator_Should_Fail_When_AvailableAmountIsNegative()
    {
        var validator = new UpdateInventoryItemValidator();
        var model = new UpdateInventoryItemModel { AvailableAmount = -1 };

        var result = validator.Validate(model);

        Assert.False(result.IsValid);
        Assert.Contains(result.Errors, e => e.PropertyName == "AvailableAmount" && e.ErrorMessage.Contains("Available amount must be 0 or greater."));
    }

    private IFormFile CreateMockFormFile(string fileName, string contentType, string content)
    {
        var stream = new MemoryStream(System.Text.Encoding.UTF8.GetBytes(content));
        var file = Substitute.For<IFormFile>();

        file.FileName.Returns(fileName);
        file.ContentType.Returns(contentType);
        file.Length.Returns(stream.Length);
        file.OpenReadStream().Returns(stream);

        return file;
    }
}
