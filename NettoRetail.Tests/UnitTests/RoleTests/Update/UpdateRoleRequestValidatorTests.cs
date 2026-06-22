using FluentAssertions;
using FluentValidation.TestHelper;
using NettoRetail.Application.BusinessRules.Roles;
using NettoRetail.Application.CQRS.Roles.Commands;
using NettoRetail.Application.Exceptions;
using NettoRetail.Application.Models.Request.Role;
using NettoRetail.DataAccess.Repositories;
using NettoRetail.DataAccess.Specifications.Permissions;
using NettoRetail.DataAccess.Specifications.Role;
using NettoRetail.DataAccess.Specifications.Shop;
using NettoRetail.Domain.Entities;
using NettoRetail.Shared.Constants;
using NettoRetail.Shared.Interfaces.Repositories;
using NettoRetail.Shared.Specifications;
using NSubstitute;

namespace NettoRetail.Tests.UnitTests.RoleTests.Update;

public class RoleUpdateRequestValidatorTests
{
    private readonly RoleUpdateCommandValidator  _validator;
    private readonly IRoleRepository _roleRepository;
    private readonly IShopRepository _shopRepository;
    private readonly IPermissionRepository _permissionRepository;

    public RoleUpdateRequestValidatorTests()
    {
        _roleRepository = Substitute.For<IRoleRepository>();
        _shopRepository = Substitute.For<IShopRepository>();
        _permissionRepository = Substitute.For<IPermissionRepository>();
        _validator = new RoleUpdateCommandValidator(roleRepository: _roleRepository, shopRepository: _shopRepository,_permissionRepository);
    }

    [Fact]
    public async Task Validate_ShouldHaveError_When_RoleIsAdmin()
    {
        var id = RoleConstants.Admin;
        var shopId = 10;
        var model = new UpdateRoleRequest
        {
            Name = "ValidName",
            Description = "ValidDescription",
            PermissionIds = [1,2,3]
        };
        var command = new RoleUpdateCommand(id, shopId, model);
        var act = async () => await _validator.ValidateAsync(command);
        await act.Should().ThrowAsync<ConflictException>()
            .WithMessage(ValidationErrorMessages.Role.CannotEditAdminRoleErrorMessage);
    }

    [Fact]
    public async Task Validate_ShouldHaveError_When_ShopDoesntExist()
    {
        var command = SetupValidUpdateRoleCommand();
        _shopRepository.ExistsAsync(Arg.Any<GetShopByIdSpecification>(), CancellationToken.None).Returns(false);
        var act = async () => await _validator.ValidateAsync(command);
        await act.Should().ThrowAsync<NotFoundException>()
            .WithMessage(string.Format(ValidationErrorMessages.Shop.ShopDoesntExistErrorMessage, command.ShopId));

    }
    
    [Fact]
    public async Task Validate_ShouldHaveError_When_RoleDoesntExist()
    {
        var command = SetupValidUpdateRoleCommand();
        _roleRepository.ExistsAsync(Arg.Any<GetRoleByIdAndShopIdSpecification>(), CancellationToken.None).Returns(false);
        var act = async () => await _validator.ValidateAsync(command);
        await act.Should().ThrowAsync<NotFoundException>()
            .WithMessage(string.Format(ValidationErrorMessages.Role.RoleNotFoundWithShopIdErrorMessage, command.Id, command.ShopId));

    }
    
    [Fact]
    public async Task Validate_ShouldHaveError_When_RoleNameIsNotUnique()
    {
        var command = SetupValidUpdateRoleCommand();
        _roleRepository.ExistsAsync(Arg.Any<GetRoleByNameExcludingCurrentSpecification>(), CancellationToken.None).Returns(true);
        var act = async () => await _validator.ValidateAsync(command);
        await act.Should().ThrowAsync<ConflictException>()
            .WithMessage(ValidationErrorMessages.Role.RoleExistsErrorMessage);

    }
    
    [Fact]
    public async Task Validate_ShouldHaveError_When_PermissionExistenceNotValid()
    {
        var command = SetupValidUpdateRoleCommand();
        _permissionRepository
            .GetIdsBySpecAsync(
                Arg.Any<GetValidPermissionsSpecification>(),
                CancellationToken.None)
            .Returns([1, 2]);
        var act = async () => await _validator.ValidateAsync(command);
        await act.Should().ThrowAsync<BadRequestException>()
            .WithMessage(ValidationErrorMessages.Role.PermissionsDontExistErrorMessage);

    }
    
    [Fact]
    public async Task Validate_ShouldNotHaveError_When_CommandIsValid()
    {
        var command = SetupValidUpdateRoleCommand();
        var result = await _validator.TestValidateAsync(command);
        result.ShouldNotHaveAnyValidationErrors();
    }

    private RoleUpdateCommand SetupValidUpdateRoleCommand()
    {
        var id = 5;
        var shopId = 10;
        var model = new UpdateRoleRequest
        {
            Name = "ValidName",
            Description = "ValidDescription",
            PermissionIds = [1,2,3]
        };
        
        var expectedResponse = new RoleEntity
        {
            Id = id,
            ShopId = shopId,
            Name = "Test",
            Description = "Test Description",
        };
        var command = new RoleUpdateCommand(id, shopId, model);
        _shopRepository.ExistsAsync(Arg.Any<GetShopByIdSpecification>(), CancellationToken.None).Returns(true);
        _roleRepository.ExistsAsync(Arg.Any<GetRoleByIdAndShopIdSpecification>(), CancellationToken.None).Returns(true);
        _roleRepository.ExistsAsync(Arg.Any<GetRoleByNameExcludingCurrentSpecification>(), CancellationToken.None).Returns(false);
        _permissionRepository
            .GetIdsBySpecAsync(
                Arg.Any<GetValidPermissionsSpecification>(),
                CancellationToken.None)
            .Returns([1, 2, 3]);
        return command;
    }
}