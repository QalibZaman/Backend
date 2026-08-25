using System.Security.Claims;
using Microsoft.IdentityModel.Tokens;
using NettoRetail.Application.CQRS.Users.Commands.Password;
using NettoRetail.Application.Exceptions;
using NettoRetail.Application.Interfaces;
using NettoRetail.Application.Models.Request.Password;
using NettoRetail.DataAccess.Specifications.Staff;
using NettoRetail.Domain.Entities;
using NettoRetail.Domain.Enums;
using NettoRetail.Shared.Interfaces.Repositories;
using NettoRetail.Shared.Interfaces.Utilities;
using NettoRetail.Shared.Specifications.PasswordHistory;
using NettoRetail.Shared.Utilities;
using NSubstitute;
using NSubstitute.ExceptionExtensions;
using Xunit;
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

        private readonly ResetPasswordCommandHandler _handler;

        private const string ValidToken = "valid-reset-token";
        private const string ValidEmail = "user@example.com";
        private const string CurrentPassword = "OldP@ssword1";
        private const string NewPassword = "NewP@ssword1";

        public ResetPasswordCommandHandlerTests()
        {
            _accountRepository = Substitute.For<IStaffRepository>();
            _passwordHistoryRepository = Substitute.For<IPasswordHistoryRepository>();
            _sessionTerminationService = Substitute.For<ISessionTerminationService>();
            _unitOfWork = Substitute.For<IUnitOfWork>();
            _jwtManager = Substitute.For<IJwtManager>();
            _transaction = Substitute.For<IDbTransaction>();

            _unitOfWork.BeginTransaction().Returns(_transaction);

            _handler = new ResetPasswordCommandHandler(
                _accountRepository,
                _passwordHistoryRepository,
                _sessionTerminationService,
                _unitOfWork,
                _jwtManager);
        }

        [Fact]
        public async Task Handle_ShouldUpdatePasswordAndActivateAccount_WhenStatusWasCreatePassword()
        {
            // Arrange - a first-time password setup flow (account still pending activation)
            SetValidToken();
            var staff = CreateStaff(status: StaffStatusEnum.CreatePassword);

            _accountRepository
                .GetAsync(Arg.Any<GetStaffByEmailSpecification>(), Arg.Any<CancellationToken>())
                .Returns(staff);

            _passwordHistoryRepository
                .GetAllBySpecAsync(Arg.Any<GetPasswordHistoryByAccountIdSpecification>(), Arg.Any<CancellationToken>())
                .Returns(Enumerable.Empty<PasswordHistoryEntity>());

            var command = new ResetPasswordCommand(new ResetPasswordRequest(ValidToken, NewPassword, NewPassword));

            // Act
            await _handler.Handle(command, CancellationToken.None);

            // Assert
            await _passwordHistoryRepository.Received(1)
                .AddAsync(Arg.Is<PasswordHistoryEntity>(h => h.AccountId == staff.Id), Arg.Any<CancellationToken>());

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
            SetValidToken();
            var staff = CreateStaff(status: StaffStatusEnum.Active);

            _accountRepository
                .GetAsync(Arg.Any<GetStaffByEmailSpecification>(), Arg.Any<CancellationToken>())
                .Returns(staff);

            _passwordHistoryRepository
                .GetAllBySpecAsync(Arg.Any<GetPasswordHistoryByAccountIdSpecification>(), Arg.Any<CancellationToken>())
                .Returns(Enumerable.Empty<PasswordHistoryEntity>());

            var command = new ResetPasswordCommand(new ResetPasswordRequest(ValidToken, NewPassword, NewPassword));

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

            var command = new ResetPasswordCommand(new ResetPasswordRequest("expired-token", NewPassword, NewPassword));

            // Act
            var act = () => _handler.Handle(command, CancellationToken.None);

            // Assert
            await Assert.ThrowsAsync<SecurityTokenExpiredException>(act);

            // Nothing past token validation should have run
            await _accountRepository.DidNotReceiveWithAnyArgs()
                .GetAsync(default!, default);
            _unitOfWork.DidNotReceive().BeginTransaction();
        }

        [Fact]
        public async Task Handle_ShouldThrowBadRequestException_WhenTokenHasNoEmailClaim()
        {
            // Arrange - token is structurally valid but missing the email claim
            var principalWithoutEmail = new ClaimsPrincipal(new ClaimsIdentity());
            _jwtManager.ValidateResetToken(Arg.Any<string>()).Returns(principalWithoutEmail);

            var command = new ResetPasswordCommand(new ResetPasswordRequest(ValidToken, NewPassword, NewPassword));

            // Act
            var act = () => _handler.Handle(command, CancellationToken.None);

            // Assert
            var exception = await Assert.ThrowsAsync<BadRequestException>(act);
            Assert.Equal("Invalid token payload.", exception.Message);

            await _accountRepository.DidNotReceiveWithAnyArgs().GetAsync(default!, default);
        }

        [Fact]
        public async Task Handle_ShouldThrowNotFoundException_WhenUserDoesNotExist()
        {
            // Arrange - token is valid but no staff account matches the email
            SetValidToken();

            _accountRepository
                .GetAsync(Arg.Any<GetStaffByEmailSpecification>(), Arg.Any<CancellationToken>())
                .Returns((StaffEntity?)null);

            var command = new ResetPasswordCommand(new ResetPasswordRequest(ValidToken, NewPassword, NewPassword));

            // Act
            var act = () => _handler.Handle(command, CancellationToken.None);

            // Assert
            var exception = await Assert.ThrowsAsync<NotFoundException>(act);
            Assert.Equal("User not found.", exception.Message);

            _unitOfWork.DidNotReceive().BeginTransaction();
        }

        [Fact]
        public async Task Handle_ShouldThrowBadRequestException_WhenNewPasswordMatchesCurrentPassword()
        {
            // Arrange - new password is identical to the one currently set
            SetValidToken();
            var staff = CreateStaff(status: StaffStatusEnum.Active);

            _accountRepository
                .GetAsync(Arg.Any<GetStaffByEmailSpecification>(), Arg.Any<CancellationToken>())
                .Returns(staff);

            var command = new ResetPasswordCommand(new ResetPasswordRequest(ValidToken, CurrentPassword, CurrentPassword));

            // Act
            var act = () => _handler.Handle(command, CancellationToken.None);

            // Assert
            var exception = await Assert.ThrowsAsync<BadRequestException>(act);
            Assert.Equal("New password cannot be the same as the current password", exception.Message);

            _unitOfWork.DidNotReceive().BeginTransaction();
        }

        [Fact]
        public async Task Handle_ShouldThrowBadRequestException_WhenNewPasswordWasUsedBefore()
        {
            // Arrange - new password matches an entry in the account's password history
            SetValidToken();
            var staff = CreateStaff(status: StaffStatusEnum.Active);

            _accountRepository
                .GetAsync(Arg.Any<GetStaffByEmailSpecification>(), Arg.Any<CancellationToken>())
                .Returns(staff);

            var previouslyUsedHistory = new PasswordHistoryEntity
            {
                AccountId = staff.Id,
                HashedPassword = PasswordHasher.HashPassword(NewPassword),
                CreatedAt = DateTime.UtcNow.AddDays(-30)
            };

            _passwordHistoryRepository
                .GetAllBySpecAsync(Arg.Any<GetPasswordHistoryByAccountIdSpecification>(), Arg.Any<CancellationToken>())
                .Returns(new[] { previouslyUsedHistory });

            var command = new ResetPasswordCommand(new ResetPasswordRequest(ValidToken, NewPassword, NewPassword));

            // Act
            var act = () => _handler.Handle(command, CancellationToken.None);

            // Assert
            var exception = await Assert.ThrowsAsync<BadRequestException>(act);
            Assert.Equal("New password cannot match any previously used passwords.", exception.Message);

            _unitOfWork.DidNotReceive().BeginTransaction();
        }

        [Fact]
        public async Task Handle_ShouldRollbackAndRethrow_WhenSessionTerminationFails()
        {
            // Arrange - simulate an unexpected failure mid-transaction
            SetValidToken();
            var staff = CreateStaff(status: StaffStatusEnum.Active);

            _accountRepository
                .GetAsync(Arg.Any<GetStaffByEmailSpecification>(), Arg.Any<CancellationToken>())
                .Returns(staff);

            _passwordHistoryRepository
                .GetAllBySpecAsync(Arg.Any<GetPasswordHistoryByAccountIdSpecification>(), Arg.Any<CancellationToken>())
                .Returns(Enumerable.Empty<PasswordHistoryEntity>());

            _sessionTerminationService
                .TerminateSessionsAsync(Arg.Any<Guid>(), Arg.Any<CancellationToken>())
                .ThrowsAsync(new InvalidOperationException("Session service unavailable"));

            var command = new ResetPasswordCommand(new ResetPasswordRequest(ValidToken, NewPassword, NewPassword));

            // Act
            var act = () => _handler.Handle(command, CancellationToken.None);

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

        // Builds a staff account whose current password is CurrentPassword
        private static StaffEntity CreateStaff(StaffStatusEnum status)
        {
            return new StaffEntity
            {
                Id = Guid.NewGuid(),
                FirstName = "First",
                LastName = "Last",
                Email = ValidEmail,
                PasswordHash = PasswordHasher.HashPassword(CurrentPassword),
                Status = status
            };
        }
    }
}