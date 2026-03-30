using FluentValidation.TestHelper;
using NettoRetail.API.Infrastructure.Validators;
using NettoRetail.Application.Models.Request.Role;
using NettoRetail.Shared.Constants;

namespace NettoRetail.Tests.UnitTests.RoleTests.Create;

public class CreateRoleRequestValidatorTests
{

    private readonly CreateRoleRequestValidator _validator;
    public CreateRoleRequestValidatorTests()
    {
        _validator = new CreateRoleRequestValidator();
    }

    [Fact]
    public async Task Validate_Should_Have_Error_When_Name_Is_Invalid()
    {
        var model = new CreateRoleRequest { Name = "a!@#", Description = "Valid description", PermissionIds = new[] { 1, 2 } };
        var result = await _validator.TestValidateAsync(model);
        result.ShouldHaveValidationErrorFor(x => x.Name)
            .WithErrorMessage(ValidationErrorMessages.Role.NameAllowedCharactersRegex);
    }

    [Theory]
    [InlineData("b")]
    [InlineData("bbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbb")]
    public async Task Validate_Should_Have_Error_When_Name_Doesnt_Meet_Length(string name)
    {
        var model = new CreateRoleRequest { Name = name, Description = "Valid description", PermissionIds = new[] { 1, 2 } };
        var result = await _validator.TestValidateAsync(model);
        result.ShouldHaveValidationErrorFor(x => x.Name)
            .WithErrorMessage(ValidationErrorMessages.Role.NameLengthErrorMessage);
    }

    [Fact]
    public async Task Validate_Should_Have_Error_When_Description_Exceeds_Max_Length()
    {
        var model = new CreateRoleRequest { Name = "ValidName", Description = new string('a', 501), PermissionIds = new[] { 1, 2 } };
        var result =await _validator.TestValidateAsync(model);
        result.ShouldHaveValidationErrorFor(x => x.Description)
            .WithErrorMessage(ValidationErrorMessages.Role.DescriptionMaxLengthErrorMessage);
    }

    [Fact]
    public async Task Validate_Should_Have_Error_When_Description_Has_Invalid_Characters()
    {
        var model = new CreateRoleRequest { Name = "ValidName", Description = "Invalid description with @", PermissionIds = new[] { 1, 2 } };
        var result = await _validator.TestValidateAsync(model);
        result.ShouldHaveValidationErrorFor(x => x.Description)
            .WithErrorMessage(ValidationErrorMessages.Role.DescriptionAllowedCharactersRegex);
    }

    [Fact]
    public async Task Validate_Should_Have_Error_When_PermissionIds_Is_Null()
    {
        var model = new CreateRoleRequest { Name = "ValidName", Description = "Valid description", PermissionIds = null };
        var result = await _validator.TestValidateAsync(model);
        result.ShouldHaveValidationErrorFor(x => x.PermissionIds)
            .WithErrorMessage(ValidationErrorMessages.Role.PermissionsListNullErrorMessage);
    }

    [Fact]
    public async Task Validate_Should_Have_Error_When_PermissionIds_Is_Empty()
    {
        var model = new CreateRoleRequest { Name = "ValidName", Description = "Valid description", PermissionIds = Array.Empty<int>() };
        var result = await _validator.TestValidateAsync(model);
        result.ShouldHaveValidationErrorFor(x => x.PermissionIds)
            .WithErrorMessage(ValidationErrorMessages.Role.PermissionsListEmptyErrorMessage);
    }

    [Fact]
    public async Task Validate_Should_Pass_When_Model_Is_Valid()
    {
        var model = new CreateRoleRequest { Name = "ValidName", Description = "Valid description", PermissionIds = new[] { 1, 2 } };
        var result = await _validator.TestValidateAsync(model);
        result.ShouldNotHaveValidationErrorFor(x => x.Name);
        result.ShouldNotHaveValidationErrorFor(x => x.Description);
        result.ShouldNotHaveValidationErrorFor(x => x.PermissionIds);
    }
}
