using FluentValidation.TestHelper;
using NettoRetail.API.Infrastructure.Validators;
using NettoRetail.Application.Models.Request.Address;
using NettoRetail.Shared.Constants;

namespace NettoRetail.Tests.UnitTests.AddressTests.Create;

public class CreateAddressRequestValidatorTests
{
    private readonly CreateAddressRequestValidator _validator;

    public CreateAddressRequestValidatorTests()
    {
        _validator = new CreateAddressRequestValidator();
    }

    [Fact]
    public void Validator_Should_Pass_When_Request_Is_Valid()
    {
        // Arrange
        var model = CreateRequest();

        // Act
        var result = _validator.TestValidate(model);

        // Assert
        result.ShouldNotHaveAnyValidationErrors();
    }

    [Theory]
    [InlineData("london", ValidationErrorMessages.Address.InvalidCityFormatErrorMessage)]
    [InlineData("L", "The length of 'City' must be at least 3 characters. You entered 1 characters.")]
    [InlineData("London123", ValidationErrorMessages.Address.InvalidCityFormatErrorMessage)]
    public void Validator_Should_Fail_When_City_Is_Invalid(string city, string expectedError)
    {
        // Arrange
        var model = CreateRequest(city: city);

        // Act
        var result = _validator.TestValidate(model);

        // Assert
        result.ShouldHaveValidationErrorFor(x => x.City)
            .WithErrorMessage(expectedError);
    }

    [Fact]
    public void Validator_Should_Fail_When_Both_Office_And_Apartment_Are_Provided()
    {
        // Arrange
        var model = CreateRequest();
        model.OfficeNumber = 101;
        model.ApartmentNumber = 5;

        // Act
        var result = _validator.TestValidate(model);

        // Assert
        result.ShouldHaveValidationErrorFor(x => x.OfficeNumber)
            .WithErrorMessage(ValidationErrorMessages.Address.OfficeNumberAndApartmentNumberConflictErrorMessage);
    }

    [Fact]
    public void Validator_Should_Fail_When_BuildingNumber_Contains_Invalid_Characters()
    {
        // Arrange
        var model = CreateRequest(buildingNumber: "51_A");

        // Act
        var result = _validator.TestValidate(model);

        // Assert
        result.ShouldHaveValidationErrorFor(x => x.BuildingNumber)
            .WithErrorMessage(ValidationErrorMessages.Address.InvalidBuildingNumberFormatErrorMessage);
    }

    [Fact]
    public void Validator_Should_Fail_When_Address_Has_Restricted_Spaces()
    {
        // Arrange
        var model = CreateRequest(street: " Main Street ");

        // Act
        var result = _validator.TestValidate(model);

        // Assert
        result.ShouldHaveAnyValidationError()
            .WithErrorMessage(ValidationErrorMessages.Address.InvalidStreetFormatErrorMessage);
    }

    private CreateAddressRequest CreateRequest(
        string zipCode = "12345",
        string city = "London",
        string street = "Baker Street",
        string buildingNumber = "221B")
    {
        return new CreateAddressRequest
        {
            CountryId = 1,
            ZipCode = zipCode,
            City = city,
            Street = street,
            BuildingNumber = buildingNumber,
            Floor = 1,
            Entrance = 1,
            ApartmentNumber = 10,
            OfficeNumber = null
        };
    }

}