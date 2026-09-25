using Microsoft.AspNetCore.Identity;
using NettoRetail.Application.CQRS.Users.Commands.Auth;
using NettoRetail.Application.Exceptions;
using NettoRetail.Application.Interfaces;
using NettoRetail.Application.Models.Authentication;
using NettoRetail.Application.Models.Response;
using NettoRetail.DataAccess.Identity;
using NettoRetail.DataAccess.Specifications.Staff;
using NettoRetail.Domain.Entities;
using NettoRetail.Domain.Enums;
using NettoRetail.Shared.Constants;
using NettoRetail.Shared.Interfaces.Repositories;
using NSubstitute;

namespace NettoRetail.Tests.UnitTests.StaffTests.LogIn;

public class LoginCommandHandlerTests
{
    private readonly UserManager<ApplicationUser> _userManager;
    private readonly IFailedAuthAttemptRepository _failedAuthAttemptRepository;
    private readonly IFailedAuthAttemptService _loginAttemptService;
    private readonly IJwtManager _jwtManager;
    private readonly IStaffRepository _staffRepository;
    private readonly LoginCommandHandler _handler;

    public LoginCommandHandlerTests()
    {
        var userStore = Substitute.For<IUserStore<ApplicationUser>>();
        _userManager = Substitute.For<UserManager<ApplicationUser>>(
            userStore, null, null, null, null, null, null, null, null);
        _failedAuthAttemptRepository = Substitute.For<IFailedAuthAttemptRepository>();
        _loginAttemptService = Substitute.For<IFailedAuthAttemptService>();
        _jwtManager = Substitute.For<IJwtManager>();
        _staffRepository = Substitute.For<IStaffRepository>();
        _handler = new LoginCommandHandler(
            _userManager,
            _failedAuthAttemptRepository,
            _loginAttemptService,
            _jwtManager,
            _staffRepository);
    }
    [Fact]
    public async Task Handler_ShouldThrowMyUnauthorizedException_WhenEmailIsNotValid()
    {
        // Arrange
        var model = new LoginModel("email@mail.com", "some-password");
        var command = new LoginCommand(model);
        _userManager.FindByEmailAsync(command.Model.Email).Returns(Task.FromResult<ApplicationUser>(null));

        // Act
        var exception = await Assert.ThrowsAnyAsync<MyUnauthorizedException>(async () =>
        {
            await _handler.Handle(command, CancellationToken.None);
        });
        
        
        // Assert
        Assert.Equal(ValidationErrorMessages.Staff.InvalidLoginErrorMessage, exception.Message);
        
        //_loginAttemptService.DidNotReceive().IsAccountLocked(Arg.Any<FailedAuthAttemptEntity>());
        await _staffRepository.DidNotReceive().GetAsync(Arg.Any<GetStaffByIdSpecification>(), Arg.Any<CancellationToken>());
        //await _failedAuthAttemptRepository.DidNotReceive().GetAsync(Arg.Any<GetFailedAuthAttemptByStaffIdSpecification>(), Arg.Any<CancellationToken>());
        _jwtManager.DidNotReceive().GetTokenResponse(Arg.Any<string>(), Arg.Any<string>(), Arg.Any<string>(),  Arg.Any<int>());
    }

    //[Fact]
    // public async Task Handler_ShouldThrowMyUnauthorizedException_IfAccountLocked()
    // {
    //     // Arrange
    //     var model = new LoginModel("email@mail.com", "some-password");
    //     var command = new LoginCommand(model);
    //     var id = Guid.NewGuid();
    //     var applicationUser = new ApplicationUser
    //     {
    //         UserName = model.Email,
    //         Email = model.Email,
    //         Id = id
    //     };
    //     var staffEntity = new StaffEntity
    //     {
    //         Id = id,
    //         FirstName = "Name",
    //         LastName = "LastName",
    //         Email = model.Email,
    //     };
    //     var loginAttempt = new FailedAuthAttemptEntity
    //     {
    //         Id = Guid.NewGuid(),
    //         FailedAttempts = 3,
    //         LockoutEndTime = DateTime.UtcNow,
    //         Staff = staffEntity,
    //     };
    //     _userManager.FindByEmailAsync(command.Model.Email).Returns(applicationUser);
    //     _staffRepository.GetAsync(Arg.Any<GetStaffByIdSpecification>(), Arg.Any<CancellationToken>()).Returns(staffEntity);
    //     _failedAuthAttemptRepository.GetAsync(Arg.Any<GetFailedAuthAttemptByStaffIdSpecification>(), Arg.Any<CancellationToken>()).Returns(loginAttempt);
    //     _loginAttemptService.IsAccountLocked(loginAttempt).Returns(true);
    //     
    //     // Act
    //     var exception = await Assert.ThrowsAsync<MyUnauthorizedException>(async () =>
    //         {
    //             await _handler.Handle(command, CancellationToken.None);
    //         }
    //     );
    //     
    //     // Assert
    //     Assert.Equal(
    //         ValidationErrorMessages.Staff.AccountLockedErrorMessage,
    //         exception.Message);
    //     await _userManager.DidNotReceive().CheckPasswordAsync(Arg.Any<ApplicationUser>(), Arg.Any<string>());
    //     _jwtManager.DidNotReceive().GetTokenResponse(Arg.Any<string>(), Arg.Any<string>(), Arg.Any<string>(),  Arg.Any<int>());
    // }

    [Fact]
    public async Task Handler_ShouldThrowBadRequestException_WhenPasswordIsInvalid()
    {
        // Arrange
        var model = new LoginModel("email@mail.com", "some-password");
        var command = new LoginCommand(model);
        var id = Guid.NewGuid();
        var applicationUser = new ApplicationUser
        {
            UserName = model.Email,
            Email = model.Email,
            Id = id
        };
        var staffEntity = new StaffEntity
        {
            Id = id,
            FirstName = "Name",
            LastName = "LastName",
            Email = model.Email,
        };
        var loginAttempt = new FailedAuthAttemptEntity
        {
            Id = Guid.NewGuid(),
            FailedAttempts = 0,
            LockoutEndTime = DateTime.UtcNow,
            Staff = staffEntity,
        };
        var updatedLoginAttempt = new FailedAuthAttemptEntity
        {
            Id = loginAttempt.Id,
            FailedAttempts = 1,
            LockoutEndTime = loginAttempt.LockoutEndTime,
            Staff = staffEntity
        };
        _userManager.FindByEmailAsync(command.Model.Email).Returns(applicationUser);
        _staffRepository.GetAsync(Arg.Any<GetStaffByIdSpecification>(), Arg.Any<CancellationToken>()).Returns(staffEntity);
        // _failedAuthAttemptRepository.GetAsync(Arg.Any<GetFailedAuthAttemptByStaffIdSpecification>(), Arg.Any<CancellationToken>()).Returns(loginAttempt);
        //_loginAttemptService.IsAccountLocked(loginAttempt).Returns(false);
        _userManager.CheckPasswordAsync(applicationUser,model.Password).Returns(Task.FromResult(false));
        // _loginAttemptService
        //     .IncrementFailedAttemptsAsync(
        //         loginAttempt,
        //         applicationUser.Id,
        //         Arg.Any<CancellationToken>())
        //     .Returns(updatedLoginAttempt);
        //
        // _loginAttemptService
        //     .AvailableAttempts(updatedLoginAttempt)
        //     .Returns(3);
        
        // Act
        var exception = await Assert.ThrowsAnyAsync<MyUnauthorizedException>(async () =>
        {
            await _handler.Handle(command, CancellationToken.None);
        });
        
        
        // Assert
        Assert.Equal(ValidationErrorMessages.Staff.InvalidLoginErrorMessage, exception.Message);
        
        _jwtManager.DidNotReceive().GetTokenResponse(Arg.Any<string>(), Arg.Any<string>(), Arg.Any<string>(),  Arg.Any<int>());
    }

    // [Fact]
    // public async Task Handler_ShouldThrowMyUnauthorizedException_WhenNoAvailableAttemptsLeft()
    // {
    //     // Arrange
    //     var model = new LoginModel("email@mail.com", "some-password");
    //     var command = new LoginCommand(model);
    //     var id = Guid.NewGuid();
    //     var applicationUser = new ApplicationUser
    //     {
    //         UserName = model.Email,
    //         Email = model.Email,
    //         Id = id
    //     };
    //     var staffEntity = new StaffEntity
    //     {
    //         Id = id,
    //         FirstName = "Name",
    //         LastName = "LastName",
    //         Email = model.Email,
    //     };
    //     var loginAttempt = new FailedAuthAttemptEntity
    //     {
    //         Id = Guid.NewGuid(),
    //         FailedAttempts = 2,
    //         LockoutEndTime = DateTime.UtcNow,
    //         Staff = staffEntity,
    //     };
    //     var updatedLoginAttempt = new FailedAuthAttemptEntity
    //     {
    //         Id = loginAttempt.Id,
    //         FailedAttempts = 3,
    //         LockoutEndTime = loginAttempt.LockoutEndTime,
    //         Staff = staffEntity
    //     };
    //     _userManager.FindByEmailAsync(command.Model.Email).Returns(applicationUser);
    //     _staffRepository.GetAsync(Arg.Any<GetStaffByIdSpecification>(), Arg.Any<CancellationToken>()).Returns(staffEntity);
    //     _failedAuthAttemptRepository.GetAsync(Arg.Any<GetFailedAuthAttemptByStaffIdSpecification>(), Arg.Any<CancellationToken>()).Returns(loginAttempt);
    //     _loginAttemptService.IsAccountLocked(loginAttempt).Returns(false);
    //     _userManager.CheckPasswordAsync(applicationUser,model.Password).Returns(Task.FromResult(false));
    //     _loginAttemptService
    //         .IncrementFailedAttemptsAsync(
    //             loginAttempt,
    //             applicationUser.Id,
    //             Arg.Any<CancellationToken>())
    //         .Returns(updatedLoginAttempt);
    //
    //     _loginAttemptService
    //         .AvailableAttempts(updatedLoginAttempt)
    //         .Returns(0);
    //     
    //     // Act
    //     var exception = await Assert.ThrowsAsync<MyUnauthorizedException>(async () =>
    //         {
    //             await _handler.Handle(command, CancellationToken.None);
    //         }
    //     );
    //     
    //     // Assert
    //     Assert.Equal(
    //         ValidationErrorMessages.Staff.LockedAccountEmailErrorMessage,
    //         exception.Message);
    //     
    //     _jwtManager.DidNotReceive().GetTokenResponse(Arg.Any<string>(), Arg.Any<string>(), Arg.Any<string>(),  Arg.Any<int>());
    // }
    [Fact]
    public async Task Handler_ShouldReturnTokenResponse_WhenLoginIsSuccessful()
    {
        // Arrange
        var model = new LoginModel("email@mail.com", "some-password");
        var command = new LoginCommand(model);

        var id = Guid.NewGuid();

        var applicationUser = new ApplicationUser
        {
            UserName = model.Email,
            Email = model.Email,
            Id = id
        };

        var staffEntity = new StaffEntity
        {
            Id = id,
            FirstName = "Name",
            LastName = "LastName",
            Email = model.Email,
    };

        var loginAttempt = new FailedAuthAttemptEntity
        {
            Id = Guid.NewGuid(),
            FailedAttempts = 1,
            LockoutEndTime = DateTime.UtcNow,
            Staff = staffEntity
        };

        var tokenResponse = new TokenResponse("some-access-token", "some-refresh-token", "some-temp-token");

        _userManager
            .FindByEmailAsync(command.Model.Email)
            .Returns(applicationUser);

        _staffRepository
            .GetAsync(
                Arg.Any<GetStaffByIdSpecification>(),
                Arg.Any<CancellationToken>())
        .   Returns(staffEntity);

        // _failedAuthAttemptRepository
        //     .GetAsync(
        //         Arg.Any<GetFailedAuthAttemptByStaffIdSpecification>(),
        //     Arg.Any<CancellationToken>())
        //     .Returns(loginAttempt);

        // _loginAttemptService
        //     .IsAccountLocked(loginAttempt)
        //     .Returns(false);
        //
        // _loginAttemptService
        //     .HasBlockDurationElapsed(loginAttempt)
        //     .Returns(false);

        _userManager
            .CheckPasswordAsync(applicationUser, model.Password)
            .Returns(true);

        _jwtManager
            .GetTokenResponse(
                id.ToString(),
                applicationUser.Email,
                staffEntity.RoleId.ToString(),
                null)
            .Returns(tokenResponse);

        // Act
        var result = await _handler.Handle(
            command,
            CancellationToken.None);

        // Assert
        Assert.Same(tokenResponse, result);

        await _userManager
            .Received(1)
            .CheckPasswordAsync(
                applicationUser,
                model.Password);

        // await _loginAttemptService
        //     .Received(1)
        //     .ResetFailedAttemptsAsync(
        //         loginAttempt,
        //         Arg.Any<CancellationToken>());

        _jwtManager
            .Received(1)
            .GetTokenResponse(
                id.ToString(),
                applicationUser.Email,
                staffEntity.RoleId.ToString(),
                null);
    }
    
}