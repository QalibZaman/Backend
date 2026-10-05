using MapsterMapper;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Options;
using NettoRetail.Application.CQRS.Users.Commands.Staff;
using NettoRetail.Application.Exceptions;
using NettoRetail.Application.Interfaces;
using NettoRetail.Application.Models.Admin;
using NettoRetail.Application.Models.Response;
using NettoRetail.DataAccess.Identity;
using NettoRetail.Domain.Entities;
using NettoRetail.Shared.Constants;
using NettoRetail.Shared.Interfaces.Repositories;
using NettoRetail.Shared.Options;
using NSubstitute;

namespace NettoRetail.Tests.UnitTests.StaffTests.Create;

public class CreateAdminCommandHandlerTests
{
    private readonly IStaffRepository _staffRepository;
    private readonly UserManager<ApplicationUser> _userManager;
    private readonly IJwtManager _jwtManager;
    private readonly IOptions<AdminRoleOptions> _adminOptions;
    private readonly IMapper _mapper;
    private readonly CreateAdminCommandHandler _handler;
    

    public  CreateAdminCommandHandlerTests()
    {
        _staffRepository = Substitute.For<IStaffRepository>();
        var userStore = Substitute.For<IUserStore<ApplicationUser>>();

        _userManager = Substitute.For<UserManager<ApplicationUser>>(
            userStore, null, null, null, null, null, null, null, null);

        _jwtManager = Substitute.For<IJwtManager>();
        _adminOptions = Substitute.For<IOptions<AdminRoleOptions>>();
        
        _mapper = Substitute.For<IMapper>();
        
        _handler = new CreateAdminCommandHandler(_staffRepository, _userManager, _jwtManager, _adminOptions, _mapper);
    }

    [Fact]
    public async Task Handler_ShouldThrowConflictException_WhenEmailExist()
    {
        // Arrange
        var model = new AdminSignUpModel
        {
            Email = "test@gmail.com",
            FirstName = "Test",
            LastName = "Test",
            Password = "password123@"
        };
        var applicationUser = new ApplicationUser
        {
            Id = Guid.NewGuid(),
            Email = model.Email,
            UserName = model.Email

        };
        var command = new CreateAdminCommand(model);
        _userManager.FindByEmailAsync(command.Model.Email).Returns(applicationUser);
        
        //Act
        var exception = await Assert.ThrowsAsync<ConflictException>(async () => await _handler.Handle(command, CancellationToken.None));
        
        //Assert
        Assert.Equal(HandlerConstants.Admin.AccountAlreadyExistsMessage, exception.Message);
        await _userManager.DidNotReceive().CreateAsync(Arg.Any<ApplicationUser>());
        await _staffRepository.DidNotReceive().AddAsync(Arg.Any<StaffEntity>(), CancellationToken.None);
        _mapper.DidNotReceive().Map<StaffEntity>(Arg.Any<AdminSignUpModel>());
        _jwtManager.DidNotReceive().GetTokenResponse(Arg.Any<string>(), Arg.Any<string>(), Arg.Any<string>(), Arg.Any<int>());
        
    }
    [Fact]
    public async Task Handler_ShouldCreateNewUser_WhenEmailDoesNotExist()
    {
        // Arrange
        var model = new AdminSignUpModel
        {
            Email = "test@gmail.com",
            FirstName = "Test",
            LastName = "Test",
            Password = "password123@"
        };
        var command = new CreateAdminCommand(model);

        _userManager.FindByEmailAsync(command.Model.Email).Returns((ApplicationUser?)null);

        _userManager.CreateAsync(Arg.Any<ApplicationUser>(), model.Password)
            .Returns(IdentityResult.Success);

        var staffEntity = new StaffEntity
        {
            Email = model.Email,
            FirstName = model.FirstName,
            LastName = model.LastName,
        };
        _mapper.Map<StaffEntity>(model).Returns(staffEntity);
        _adminOptions.Value.Returns(new AdminRoleOptions
        {
            RoleId = 1,
            PermissionIds = [ 1, 2, 3, 4, 5 ]
        });
        var savedStaff = new StaffEntity
        {
            Id = Guid.NewGuid(),
            Email = model.Email,
            RoleId= _adminOptions.Value.RoleId,
            FirstName = model.FirstName,
            LastName = model.LastName
        };
        _staffRepository.AddAsync(Arg.Any<StaffEntity>(), Arg.Any<CancellationToken>())
            .Returns(savedStaff);
        
        var expectedTokenResponse = new TokenResponse("accessToken", "refreshToken", "TempToken");
        _jwtManager.GetTokenResponse(
                savedStaff.Id.ToString(),
                savedStaff.Email,
                savedStaff.RoleId.ToString(),
                Arg.Any<int?>())
            .Returns(expectedTokenResponse);
        
        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        Assert.NotNull(result);
        Assert.Equal(expectedTokenResponse, result);

        await _userManager.Received(1).CreateAsync(Arg.Any<ApplicationUser>(), model.Password);
        await _staffRepository.Received(1).AddAsync(Arg.Any<StaffEntity>(), Arg.Any<CancellationToken>());
        _mapper.Received(1).Map<StaffEntity>(model);
        _jwtManager.Received(1).GetTokenResponse(
            savedStaff.Id.ToString(), savedStaff.Email, savedStaff.RoleId.ToString(), Arg.Any<int?>());
    }
    
}