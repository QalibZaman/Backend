using FluentAssertions;
using FluentValidation;
using FluentValidation.TestHelper;
using NettoRetail.API.Infrastructure.Validators;
using NettoRetail.Application.CQRS.Address.Commands;
using NettoRetail.Application.Exceptions;
using NettoRetail.Application.Models.Request.Address;
using NettoRetail.DataAccess.Specifications.Country;
using NettoRetail.Domain.Entities;
using NettoRetail.Shared.Constants;
using NettoRetail.Shared.Interfaces.Repositories;
using NSubstitute;

namespace NettoRetail.Tests.UnitTests.AddressTests.Create;

public class CreateAddressCommandValidatorTests
{
    private readonly ICountryRepository _countryRepository;
    private readonly InlineValidator<CreateAddressRequest> _requestValidator;
    private readonly IAddressRepository _addressRepository;
    private readonly CreateAddressCommandValidator _validator;

    private const string ValidZipMask = "^[0-9]{5}$";
    private const string ValidZipCode = "12345";
    private const int ValidCountryId = 1;
    public CreateAddressCommandValidatorTests()
    {
        _countryRepository = Substitute.For<ICountryRepository>();
        _addressRepository = Substitute.For<IAddressRepository>();

        _requestValidator = new InlineValidator<CreateAddressRequest>();

        _validator = new CreateAddressCommandValidator(_countryRepository, _requestValidator, _addressRepository);
    }
    [Fact]
    public async Task Validator_Should_Pass_When_All_Conditions_Are_Valid()
    {
        // Arrange
        var command = CreateCommand();
        var country = CreateCountry(ValidCountryId, ValidZipMask);

        _addressRepository.CountByUserIdAsync(command.UserId, Arg.Any<CancellationToken>())
            .Returns(0);

        _countryRepository.GetAsync(Arg.Any<GetCountryByIdSpecification>(), Arg.Any<CancellationToken>())
            .Returns(country);

        // Act
        var result = await _validator.TestValidateAsync(command);

        // Assert
        result.ShouldNotHaveAnyValidationErrors();
    }

    [Fact]
    public async Task Validator_Should_Throw_ConflictException_When_AddressLimitReached()
    {
        // Arrange
        var command = CreateCommand();
        _addressRepository.CountByUserIdAsync(command.UserId, Arg.Any<CancellationToken>())
            .Returns(15);

        // Act
        var act = async () => await _validator.TestValidateAsync(command);

        // Assert
        await act.Should().ThrowAsync<ConflictException>()
            .WithMessage(ValidationErrorMessages.Address.MaxAddressesLimitErrorMessage);
    }

    [Fact]
    public async Task Validator_Should_Throw_BadRequestException_When_ZipCode_Mismatch()
    {
        // Arrange
        var command = CreateCommand(zipCode: "ABC-123");
        var country = CreateCountry(ValidCountryId, ValidZipMask);

        _addressRepository.CountByUserIdAsync(command.UserId, Arg.Any<CancellationToken>())
            .Returns(0);
        _countryRepository.GetAsync(Arg.Any<GetCountryByIdSpecification>(), Arg.Any<CancellationToken>())
            .Returns(country);

        // Act
        var act = async () => await _validator.TestValidateAsync(command);

        // Assert
        await act.Should().ThrowAsync<BadRequestException>()
            .WithMessage(ValidationErrorMessages.Country.InvalidZipCodeErrorMessage);
    }

    private CreateAddressCommand CreateCommand(
        int countryId = ValidCountryId,
        string zipCode = ValidZipCode,
        string city = "London",
        string street = "Baker Street",
        string buildingNumber = "221B")
    {
        return new CreateAddressCommand(Guid.NewGuid(), new CreateAddressRequest
        {
            CountryId = countryId,
            ZipCode = zipCode,
            City = city,
            Street = street,
            BuildingNumber = buildingNumber,
            Floor = 1,
            Entrance = 1,
            OfficeNumber = null,
            ApartmentNumber = 10
        });
    }

    private CountryEntity CreateCountry(int id, string zipMask)
    {
        return new CountryEntity
        {
            Id = id,
            Name = "TestCountry",
            PhoneCode = "+1",
            PhoneMask = "(123) 123-1234",
            ZipCodeMask = zipMask
        };
    }
}