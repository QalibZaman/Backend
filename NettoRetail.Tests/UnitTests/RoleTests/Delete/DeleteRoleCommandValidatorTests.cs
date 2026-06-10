using FluentAssertions;
using FluentValidation.TestHelper;
using NettoRetail.API.Infrastructure.Validators;
using NettoRetail.Application.CQRS.Roles.Commands;
using NettoRetail.Application.Exceptions;
using NettoRetail.DataAccess.Specifications.Staff;
using NettoRetail.DataAccess.Specifications.Shop;
using NettoRetail.Shared.Constants;
using NettoRetail.Shared.Interfaces.Repositories;
using NSubstitute;

namespace NettoRetail.Tests.UnitTests.RoleTests.Delete;

public class DeleteRoleCommandValidatorTests
{
    private readonly IShopRepository _shopRepository;
    private readonly IStaffRepository _staffReposirory;
    private readonly DeleteRoleCommandValidator _validator;
    public DeleteRoleCommandValidatorTests()
    {
        _shopRepository = Substitute.For<IShopRepository>();
        _staffReposirory = Substitute.For<IStaffRepository>();
        _validator = new DeleteRoleCommandValidator(_shopRepository, _staffReposirory);
    }

    [Fact]
    public async Task Validate_Should_Have_Error_When_Shop_Does_Not_Exist()
    {
        //Arrange
        var command = new DeleteRoleCommand(2, 1);
        _shopRepository
            .ExistsAsync(Arg.Any<GetShopByIdSpecification>(), Arg.Any<CancellationToken>())
            .Returns(false);

        //Act
        var act = async () => await _validator.TestValidateAsync(command);

        //Assert
        await act.Should().ThrowAsync<NotFoundException>()
            .WithMessage(string.Format(ValidationErrorMessages.Shop.ShopDoesntExistErrorMessage, command.ShopId));
    }

    [Fact]
    public async Task Validate_Should_Have_Error_When_RoleId_Is_Less_Than_Zero()
    {
        //Arrange
        var command = new DeleteRoleCommand(-23, 1);
        _shopRepository
            .ExistsAsync(Arg.Any<GetShopByIdSpecification>(), Arg.Any<CancellationToken>())
            .Returns(true);

        //Act
        var result = await _validator.TestValidateAsync(command);

        //Assert
        result.ShouldHaveValidationErrorFor(c => c.Id);
    }

    [Fact]
    public async Task Validate_Should_Have_Error_When_Role_Is_In_Use()
    {
        //Arrange
        var command = new DeleteRoleCommand(2, 1);
        _shopRepository
            .ExistsAsync(Arg.Any<GetShopByIdSpecification>(), Arg.Any<CancellationToken>())
            .Returns(true);
        _staffReposirory
            .ExistsAsync(Arg.Any<GetStaffByRoleIdSpecification>(), Arg.Any<CancellationToken>())
            .Returns(true);

        //Act
        var act = async () => await _validator.TestValidateAsync(command);

        //Assert
        await act.Should().ThrowAsync<ConflictException>()
            .WithMessage(ValidationErrorMessages.Role.RoleInUseErrorMessage);
    }

    [Fact]
    public async Task Validate_Should_Have_Error_When_Trying_To_Delete_Admin_Role()
    {
        //Arrange
        var command = new DeleteRoleCommand(RoleConstants.Admin, 1);

        //Act
        var act = async () => await _validator.TestValidateAsync(command);

        //Assert
        await act.Should().ThrowAsync<ConflictException>()
            .WithMessage(ValidationErrorMessages.Role.CannotDeleteAdminRoleErrorMessage);
    }

    [Fact]
    public async Task Validate_Should_Not_Have_Error_When_Command_Is_Valid()
    {
        //Arrange
        var command = new DeleteRoleCommand(2, 1);
        _shopRepository
            .ExistsAsync(Arg.Any<GetShopByIdSpecification>(), Arg.Any<CancellationToken>())
            .Returns(true);
        _staffReposirory
            .ExistsAsync(Arg.Any<GetStaffByRoleIdSpecification>(), Arg.Any<CancellationToken>())
            .Returns(false);

        //Act
        var result = await _validator.TestValidateAsync(command);

        //Assert
        result.ShouldNotHaveValidationErrorFor(c => c.Id);
        result.ShouldNotHaveValidationErrorFor(c => c.ShopId);
    }
}
