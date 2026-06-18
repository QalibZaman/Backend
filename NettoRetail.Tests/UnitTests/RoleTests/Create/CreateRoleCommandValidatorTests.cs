using FluentAssertions;
using FluentValidation;
using FluentValidation.TestHelper;
using NettoRetail.API.Infrastructure.Validators;
using NettoRetail.Application.CQRS.Roles.Commands;
using NettoRetail.Application.Exceptions;
using NettoRetail.Application.Models.Request.Role;
using NettoRetail.DataAccess.Specifications.Permissions;
using NettoRetail.DataAccess.Specifications.Role;
using NettoRetail.DataAccess.Specifications.Shop;
using NettoRetail.Shared.Constants;
using NettoRetail.Shared.Interfaces.Repositories;
using NSubstitute;

namespace NettoRetail.Tests.UnitTests.RoleTests.Create;

public class CreateRoleCommandValidatorTests
{
    private readonly IShopRepository _shopRepository;
    private readonly IRoleRepository _roleRepository;
    private readonly IPermissionRepository _permissionRepository;

    private readonly CreateRoleCommandValidator _validator;
    public CreateRoleCommandValidatorTests()
    {
        var createRoleRequestValidator = new InlineValidator<CreateRoleRequest>();

        _shopRepository = Substitute.For<IShopRepository>();
        _roleRepository = Substitute.For<IRoleRepository>();
        _permissionRepository = Substitute.For<IPermissionRepository>();

        _validator = new CreateRoleCommandValidator(createRoleRequestValidator, _shopRepository, _roleRepository, _permissionRepository);
    }

    [Fact]
    public async Task Validate_Should_Have_Error_When_ShopId_Is_Empty()
    {
        //Arrange
        var command = new CreateRoleCommand(new CreateRoleRequest
        {
            Name = "",
            PermissionIds = new[] { 1, 2 }
        }, 0);
        //Act
        var result = await _validator.TestValidateAsync(command);
        //Assert
        result.ShouldHaveValidationErrorFor(c => c.ShopId)
            .WithErrorMessage(ValidationErrorMessages.Shop.ShopIdRequiredErrorMessage);
    }

    [Fact]
    public async Task Validate_Should_Have_Error_When_Shop_Does_Not_Exist()
    {
        //Arrange
        var command = SetupValidCreateRoleCommand();

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
    public async Task Validate_Should_Have_Error_When_Role_Name_Not_Unique()
    {
        //Arrange
        var command = SetupValidCreateRoleCommand();

        _roleRepository.ExistsAsync(Arg.Any<GetRoleByNameAndShopIdSpecification>(), Arg.Any<CancellationToken>()).Returns(true);

        //Act
         var act = async () => await _validator.TestValidateAsync(command);

        //Assert
        await act.Should().ThrowAsync<ConflictException>()
            .WithMessage(ValidationErrorMessages.Role.RoleExistsErrorMessage);
    }

    [Fact]
    public async Task Validate_Should_Have_Error_When_Permissions_Are_Invalid()
    {
        //Arrange
        var command = SetupValidCreateRoleCommand();
        _permissionRepository.GetIdsBySpecAsync(Arg.Any<GetValidPermissionsSpecification>(), Arg.Any<CancellationToken>()).Returns(new List<int>());

        //Act
        var act = async () => await _validator.TestValidateAsync(command);

        //Assert
        await act.Should().ThrowAsync<NotFoundException>()
            .WithMessage(ValidationErrorMessages.Role.PermissionsDontExistErrorMessage);
    }

    [Fact]
    public async Task Validate_Should_Not_Have_Error_When_Command_Is_Valid()
    {
        var command = SetupValidCreateRoleCommand();
        var result = await _validator.TestValidateAsync(command);
        result.ShouldNotHaveAnyValidationErrors();
    }

    private CreateRoleCommand SetupValidCreateRoleCommand()
    {
        var command = new CreateRoleCommand(new CreateRoleRequest
        {
            Name = "ValidName",
            PermissionIds = new[] { 1, 2 }
        }, 10);
        var validPermissions = new List<int> { 1, 2 };
        _shopRepository.ExistsAsync(Arg.Any<GetShopByIdSpecification>(), Arg.Any<CancellationToken>()).Returns(true);

        _permissionRepository.GetIdsBySpecAsync(Arg.Any<GetValidPermissionsSpecification>(), Arg.Any<CancellationToken>()).Returns(validPermissions);

        return command;
    }
}
