using System.Security.Claims;
using Microsoft.AspNetCore.Identity;
using Microsoft.IdentityModel.Tokens;
using NettoRetail.Application.CQRS.Users.Commands.Password;
using NettoRetail.Application.Exceptions;
using NettoRetail.Application.Interfaces;
using NettoRetail.Application.Models.Request.Password;
using NettoRetail.DataAccess.Identity;
using NettoRetail.DataAccess.Specifications.Staff;
using NettoRetail.Domain.Entities;
using NettoRetail.Domain.Enums;
using NettoRetail.Shared.Interfaces.Repositories;
using NettoRetail.Shared.Interfaces.Utilities;
using NettoRetail.Shared.Specifications.PasswordHistory;
using NSubstitute;
using NSubstitute.ExceptionExtensions;
using System.Data;

namespace NettoRetail.Tests.UnitTests.UsersTests.PasswordTests
{
    public class ResetPasswordCommandHandlerTests
    {
        // Mocked dependencies - same pattern as LogoutCommandHandlerTests
        private readonly IStaffRepository _accountRepository;
        private readonly IPasswordHistoryRepository _passwordHistoryRepository;
        private readonly ISessionTerminationService _sessionTerminationService;
        private readonly IUnitOfWork _unitOfWork;
        private readonly IJwtManager _jwtManager;
        private readonly IDbTransaction _transaction;
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly IPasswordHasher<ApplicationUser> _passwordHasher;

        private readonly ResetPasswordCommandHandler _handler;

        private const string ValidToken = "valid-reset-token";
        private const string ValidEmail = "user@example.com";
        private const string CurrentPasswordHash = "current-hashed-password";
        private const string NewPassword = "NewP@ssword1";
        private const string NewPasswordHash = "new-hashed-password";

        public ResetPasswordCommandHandlerTests()
        {
            _accountRepository = Substitute.For<IStaffRepository>();
            _passwordHistoryRepository = Substitute.For<IPasswordHistoryRepository>();
            _sessionTerminationService = Substitute.For<ISessionTerminationService>();
            _unitOfWork = Substitute.For<IUnitOfWork>();
            _jwtManager = Substitute.For<IJwtManager>();
            _transaction = Substitute.For<IDbTransaction>();

            // UserManager<T> is a concrete class, not an interface - NSubstitute can still
            // mock it because its members are virtual. Only IUserStore is required to
            // construct it; the rest of the constructor parameters can be null for testing.
            var userStore = Substitute.For<IUserStore<ApplicationUser>>();
            _userManager = Substitute.For<UserManager<ApplicationUser>>(
                userStore, null, null, null, null, null, null, null, null);

            // PasswordHasher has a public setter on UserManager, so we can swap in our own mock
            // instead of relying on the real Identity password hashing implementation.
            _passwordHasher = Substitute.For<IPasswordHasher<ApplicationUser>>();
            _userManager.PasswordHasher = _passwordHasher;

            _unitOfWork.BeginTransaction().Returns(_transaction);

            _handler = new ResetPasswordCommandHandler(
                _accountRepository,
                _passwordHistoryRepository,
                _sessionTerminationService,
                _unitOfWork,
                _jwtManager,
                _userManager);
        }

        [Fact]
        public async Task Handle_ShouldUpdatePasswordAndActivateAccount_WhenStatusWasCreatePassword()
        {
            // Arrange - a first-time password setup flow (account still pending activation)
            var staff = CreateStaff(status: StaffStatusEnum.CreatePassword);
            var identityUser = CreateIdentityUser(staff.Id);

            SetValidToken();
            _userManager.FindByEmailAsync(ValidEmail).Returns(identityUser);
            _accountRepository
                .GetAsync(Arg.Any<GetStaffByEmailSpecification>(), Arg.Any<CancellationToken>())
                .Returns(staff);

            _passwordHasher
                .VerifyHashedPassword(identityUser, identityUser.PasswordHash!, NewPassword)
                .Returns(PasswordVerificationResult.Failed);

            _passwordHistoryRepository
                .GetAllBySpecAsync(Arg.Any<GetPasswordHistoryByAccountIdSpecification>(), Arg.Any<CancellationToken>())
                .Returns(Enumerable.Empty<PasswordHistoryEntity>());

            _passwordHasher.HashPassword(identityUser, NewPassword).Returns(NewPasswordHash);
            _userManager.UpdateAsync(identityUser).Returns(IdentityResult.Success);

            var command = new ResetPasswordCommand(new ResetPasswordRequest(ValidToken, NewPassword));

            // Act
            await _handler.Handle(command, CancellationToken.None);

            // Assert
            await _passwordHistoryRepository.Received(1)
                .AddAsync(Arg.Is<PasswordHistoryEntity>(h => h.AccountId == staff.Id), Arg.Any<CancellationToken>());

            Assert.Equal(NewPasswordHash, identityUser.PasswordHash);
            await _userManager.Received(1).UpdateAsync(identityUser);

            await _accountRepository.Received(1).UpdateAsync(
                Arg.Is<StaffEntity>(u => u.Status == StaffStatusEnum.Active && u.PasswordUpdatedAt != default),
                Arg.Any<CancellationToken>());

            await _sessionTerminationService.Received(1).TerminateSessionsAsync(staff.Id, Arg.Any<CancellationToken>());

            _transaction.Received(1).Commit();
            _transaction.DidNotReceive().Rollback();
        }

        [Fact]
        public async Task Handle_ShouldUpdatePasswordAndKeepStatusActive_WhenAccountWasAlreadyActive()
        {
            // Arrange - a regular password reset for an already-active account:
            // status must stay Active, not be changed by the reset itself.
            var staff = CreateStaff(status: StaffStatusEnum.Active);
            var identityUser = CreateIdentityUser(staff.Id);

            SetValidToken();
            _userManager.FindByEmailAsync(ValidEmail).Returns(identityUser);
            _accountRepository
                .GetAsync(Arg.Any<GetStaffByEmailSpecification>(), Arg.Any<CancellationToken>())
                .Returns(staff);

            _passwordHasher
                .VerifyHashedPassword(identityUser, identityUser.PasswordHash!, NewPassword)
                .Returns(PasswordVerificationResult.Failed);

            _passwordHistoryRepository
                .GetAllBySpecAsync(Arg.Any<GetPasswordHistoryByAccountIdSpecification>(), Arg.Any<CancellationToken>())
                .Returns(Enumerable.Empty<PasswordHistoryEntity>());

            _passwordHasher.HashPassword(identityUser, NewPassword).Returns(NewPasswordHash);
            _userManager.UpdateAsync(identityUser).Returns(IdentityResult.Success);

            var command = new ResetPasswordCommand(new ResetPasswordRequest(ValidToken, NewPassword));

            // Act
            await _handler.Handle(command, CancellationToken.None);

            // Assert
            await _accountRepository.Received(1).UpdateAsync(
                Arg.Is<StaffEntity>(u => u.Status == StaffStatusEnum.Active),
                Arg.Any<CancellationToken>());

            _transaction.Received(1).Commit();
        }

        [Fact]
        public async Task Handle_ShouldPropagateException_WhenResetTokenIsInvalidOrExpired()
        {
            // Arrange - ValidateResetToken itself throws on a bad/expired token
            // (unlike other token validators in JwtManager, this one does not
            // wrap the exception into a BadRequestException)
            _jwtManager
                .ValidateResetToken(Arg.Any<string>())
                .Throws(new SecurityTokenExpiredException("Token has expired"));

            var command = new ResetPasswordCommand(new ResetPasswordRequest("expired-token", NewPassword));

            // Act
            Task act() => _handler.Handle(command, CancellationToken.None);

            // Assert
            await Assert.ThrowsAsync<SecurityTokenExpiredException>(act);

            await _userManager.DidNotReceiveWithAnyArgs().FindByEmailAsync(default!);
            _unitOfWork.DidNotReceive().BeginTransaction();
        }

        [Fact]
        public async Task Handle_ShouldThrowBadRequestException_WhenTokenHasNoEmailClaim()
        {
            // Arrange - token is structurally valid but missing the email claim
            var principalWithoutEmail = new ClaimsPrincipal(new ClaimsIdentity());
            _jwtManager.ValidateResetToken(Arg.Any<string>()).Returns(principalWithoutEmail);

            var command = new ResetPasswordCommand(new ResetPasswordRequest(ValidToken, NewPassword));

            // Act
            Task act() => _handler.Handle(command, CancellationToken.None);

            // Assert
            var exception = await Assert.ThrowsAsync<BadRequestException>(act);
            Assert.Equal("Invalid token payload.", exception.Message);

            await _userManager.DidNotReceiveWithAnyArgs().FindByEmailAsync(default!);
        }

        [Fact]
        public async Task Handle_ShouldThrowNotFoundException_WhenIdentityUserDoesNotExist()
        {
            // Arrange - token is valid but no Identity user matches the email
            SetValidToken();
            _userManager.FindByEmailAsync(ValidEmail).Returns((ApplicationUser?)null);

            var command = new ResetPasswordCommand(new ResetPasswordRequest(ValidToken, NewPassword));

            // Act
            Task act() => _handler.Handle(command, CancellationToken.None);

            // Assert
            var exception = await Assert.ThrowsAsync<NotFoundException>(act);
            Assert.Equal("User not found.", exception.Message);

            await _accountRepository.DidNotReceiveWithAnyArgs().GetAsync(default!, default);
            _unitOfWork.DidNotReceive().BeginTransaction();
        }

        [Fact]
        public async Task Handle_ShouldThrowNotFoundException_WhenStaffAccountDoesNotExist()
        {
            // Arrange - Identity user exists, but there is no matching StaffEntity
            var identityUser = CreateIdentityUser(Guid.NewGuid());

            SetValidToken();
            _userManager.FindByEmailAsync(ValidEmail).Returns(identityUser);
            _accountRepository
                .GetAsync(Arg.Any<GetStaffByEmailSpecification>(), Arg.Any<CancellationToken>())
                .Returns((StaffEntity?)null);

            var command = new ResetPasswordCommand(new ResetPasswordRequest(ValidToken, NewPassword));

            // Act
            Task act() => _handler.Handle(command, CancellationToken.None);

            // Assert
            var exception = await Assert.ThrowsAsync<NotFoundException>(act);
            Assert.Equal("User not found.", exception.Message);

            _unitOfWork.DidNotReceive().BeginTransaction();
        }

        [Fact]
        public async Task Handle_ShouldThrowBadRequestException_WhenNewPasswordMatchesCurrentPassword()
        {
            // Arrange - new password verifies successfully against the current hash
            var staff = CreateStaff(status: StaffStatusEnum.Active);
            var identityUser = CreateIdentityUser(staff.Id);

            SetValidToken();
            _userManager.FindByEmailAsync(ValidEmail).Returns(identityUser);
            _accountRepository
                .GetAsync(Arg.Any<GetStaffByEmailSpecification>(), Arg.Any<CancellationToken>())
                .Returns(staff);

            _passwordHasher
                .VerifyHashedPassword(identityUser, identityUser.PasswordHash!, NewPassword)
                .Returns(PasswordVerificationResult.Success);

            var command = new ResetPasswordCommand(new ResetPasswordRequest(ValidToken, NewPassword));

            // Act
            Task act() => _handler.Handle(command, CancellationToken.None);

            // Assert
            var exception = await Assert.ThrowsAsync<BadRequestException>(act);
            Assert.Equal("New password cannot be the same as the current password.", exception.Message);

            _unitOfWork.DidNotReceive().BeginTransaction();
        }

        [Fact]
        public async Task Handle_ShouldThrowBadRequestException_WhenNewPasswordWasUsedBefore()
        {
            // Arrange - new password matches an entry in the account's password history
            var staff = CreateStaff(status: StaffStatusEnum.Active);
            var identityUser = CreateIdentityUser(staff.Id);

            SetValidToken();
            _userManager.FindByEmailAsync(ValidEmail).Returns(identityUser);
            _accountRepository
                .GetAsync(Arg.Any<GetStaffByEmailSpecification>(), Arg.Any<CancellationToken>())
                .Returns(staff);

            _passwordHasher
                .VerifyHashedPassword(identityUser, identityUser.PasswordHash!, NewPassword)
                .Returns(PasswordVerificationResult.Failed);

            var previouslyUsedHistory = new PasswordHistoryEntity
            {
                AccountId = staff.Id,
                HashedPassword = "old-hashed-password",
                CreatedAt = DateTime.UtcNow.AddDays(-30)
            };

            _passwordHistoryRepository
                .GetAllBySpecAsync(Arg.Any<GetPasswordHistoryByAccountIdSpecification>(), Arg.Any<CancellationToken>())
                .Returns(new[] { previouslyUsedHistory });

            _passwordHasher
                .VerifyHashedPassword(identityUser, previouslyUsedHistory.HashedPassword, NewPassword)
                .Returns(PasswordVerificationResult.Success);

            var command = new ResetPasswordCommand(new ResetPasswordRequest(ValidToken, NewPassword));

            // Act
            Task act() => _handler.Handle(command, CancellationToken.None);

            // Assert
            var exception = await Assert.ThrowsAsync<BadRequestException>(act);
            Assert.Equal("New password cannot match any previously used passwords.", exception.Message);

            _unitOfWork.DidNotReceive().BeginTransaction();
        }

        [Fact]
        public async Task Handle_ShouldThrowBadRequestException_WhenIdentityUpdateFails()
        {
            // Arrange - Identity's UpdateAsync reports validation errors (e.g. password policy)
            var staff = CreateStaff(status: StaffStatusEnum.Active);
            var identityUser = CreateIdentityUser(staff.Id);

            SetValidToken();
            _userManager.FindByEmailAsync(ValidEmail).Returns(identityUser);
            _accountRepository
                .GetAsync(Arg.Any<GetStaffByEmailSpecification>(), Arg.Any<CancellationToken>())
                .Returns(staff);

            _passwordHasher
                .VerifyHashedPassword(identityUser, identityUser.PasswordHash!, NewPassword)
                .Returns(PasswordVerificationResult.Failed);

            _passwordHistoryRepository
                .GetAllBySpecAsync(Arg.Any<GetPasswordHistoryByAccountIdSpecification>(), Arg.Any<CancellationToken>())
                .Returns(Enumerable.Empty<PasswordHistoryEntity>());

            _passwordHasher.HashPassword(identityUser, NewPassword).Returns(NewPasswordHash);

            var identityError = IdentityResult.Failed(new IdentityError { Description = "Password is too weak." });
            _userManager.UpdateAsync(identityUser).Returns(identityError);

            var command = new ResetPasswordCommand(new ResetPasswordRequest(ValidToken, NewPassword));

            // Act
            var act = () => _handler.Handle(command, CancellationToken.None);

            // Assert
            var exception = await Assert.ThrowsAsync<BadRequestException>(act);
            Assert.Equal("Password is too weak.", exception.Message);

            _transaction.Received(1).Rollback();
            _transaction.DidNotReceive().Commit();
        }

        [Fact]
        public async Task Handle_ShouldRollbackAndRethrow_WhenSessionTerminationFails()
        {
            // Arrange - simulate an unexpected failure mid-transaction
            var staff = CreateStaff(status: StaffStatusEnum.Active);
            var identityUser = CreateIdentityUser(staff.Id);

            SetValidToken();
            _userManager.FindByEmailAsync(ValidEmail).Returns(identityUser);
            _accountRepository
                .GetAsync(Arg.Any<GetStaffByEmailSpecification>(), Arg.Any<CancellationToken>())
                .Returns(staff);

            _passwordHasher
                .VerifyHashedPassword(identityUser, identityUser.PasswordHash!, NewPassword)
                .Returns(PasswordVerificationResult.Failed);

            _passwordHistoryRepository
                .GetAllBySpecAsync(Arg.Any<GetPasswordHistoryByAccountIdSpecification>(), Arg.Any<CancellationToken>())
                .Returns(Enumerable.Empty<PasswordHistoryEntity>());

            _passwordHasher.HashPassword(identityUser, NewPassword).Returns(NewPasswordHash);
            _userManager.UpdateAsync(identityUser).Returns(IdentityResult.Success);

            _sessionTerminationService
                .TerminateSessionsAsync(Arg.Any<Guid>(), Arg.Any<CancellationToken>())
                .ThrowsAsync(new InvalidOperationException("Session service unavailable"));

            var command = new ResetPasswordCommand(new ResetPasswordRequest(ValidToken, NewPassword));

            // Act
            Task act() => _handler.Handle(command, CancellationToken.None);

            // Assert
            await Assert.ThrowsAsync<InvalidOperationException>(act);

            _transaction.Received(1).Rollback();
            _transaction.DidNotReceive().Commit();
        }

        // Wires up a "reset" token whose claims resolve to ValidEmail
        private void SetValidToken()
        {
            var claimsPrincipal = new ClaimsPrincipal(new ClaimsIdentity(new[]
            {
                new Claim(ClaimTypes.Email, ValidEmail)
            }));

            _jwtManager.ValidateResetToken(ValidToken).Returns(claimsPrincipal);
        }

        // Builds a staff account (no password hash stored here anymore - Identity owns it)
        private static StaffEntity CreateStaff(StaffStatusEnum status)
        {
            return new StaffEntity
            {
                Id = Guid.NewGuid(),
                FirstName = "First",
                LastName = "Last",
                Email = ValidEmail,
                Status = status
            };
        }

        // Builds the Identity-side user record, sharing the same Id as the StaffEntity
        private static ApplicationUser CreateIdentityUser(Guid id)
        {
            return new ApplicationUser
            {
                Id = id,
                Email = ValidEmail,
                UserName = ValidEmail,
                PasswordHash = CurrentPasswordHash
            };
        }
    }
}