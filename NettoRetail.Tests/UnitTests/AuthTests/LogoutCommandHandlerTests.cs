using System.Data;
using NettoRetail.Application.CQRS.Users.Commands.Auth;
using NettoRetail.Application.Exceptions;
using NettoRetail.Application.Interfaces;
using NettoRetail.Application.Services.PermissionCache;
using NettoRetail.DataAccess.Specifications.Sessions;
using NettoRetail.Domain.Entities;
using NettoRetail.Shared.Interfaces.Repositories;
using NettoRetail.Shared.Interfaces.Utilities;
using NSubstitute;
using NSubstitute.ExceptionExtensions;

namespace NettoRetail.Tests.UnitTests.AuthTests
{
    public class LogoutCommandHandlerTests
    {
        private readonly IBlacklistTokenService _blacklistTokenService;
        private readonly ISessionRepository _sessionRepository;
        private readonly IPermissionCacheService _permissionCacheService;
        private readonly IUnitOfWork _unitOfWork;
        private readonly IDbTransaction _transaction;

        private readonly LogoutCommandHandler _handler;

        public LogoutCommandHandlerTests()
        {
            _blacklistTokenService = Substitute.For<IBlacklistTokenService>();
            _sessionRepository = Substitute.For<ISessionRepository>();
            _permissionCacheService = Substitute.For<IPermissionCacheService>();
            _unitOfWork = Substitute.For<IUnitOfWork>();
            _transaction = Substitute.For<IDbTransaction>();

            _unitOfWork.BeginTransaction().Returns(_transaction);

            _handler = new LogoutCommandHandler(
                _blacklistTokenService,
                _sessionRepository,
                _permissionCacheService,
                _unitOfWork);
        }

        [Fact]
        public async Task Handle_ShouldDeactivateSessionAndBlacklistToken_WhenRequestIsValid()
        {
            var accessToken = "valid-access-token";
            var userId = Guid.NewGuid();
            var session = CreateActiveSession();

            _sessionRepository
                .GetAsync(Arg.Any<GetSessionsByJwtSpecification>(), Arg.Any<CancellationToken>())
                .Returns(session);

            var command = new LogoutCommand(accessToken, userId);

            await _handler.Handle(command, CancellationToken.None);

            Assert.False(session.IsActive);
            await _sessionRepository.Received(1).UpdateAsync(session, Arg.Any<CancellationToken>());

            // Refresh token + its own expiry must be sent to the blacklist service
            await _blacklistTokenService
                .Received(1)
                .BlacklistTokenAsync(session.RefreshToken, session.ExpiresAt, Arg.Any<CancellationToken>());

            _transaction.Received(1).Commit();
            _transaction.DidNotReceive().Rollback();

            // Permission cache cleared for this exact session
            _permissionCacheService.Received(1).RemovePermissions(session.Id);
        }

        [Theory]
        [InlineData(null)]
        [InlineData("")]
        public async Task Handle_ShouldThrowInvalidTokenCredentialsException_WhenAccessTokenIsMissing(string? accessToken)
        {
            var command = new LogoutCommand(accessToken!, Guid.NewGuid());

            Task act() => _handler.Handle(command, CancellationToken.None);

            var exception = await Assert.ThrowsAsync<InvalidTokenCredentialsException>(act);
            Assert.Equal("Authorization token is missing or invalid.", exception.Message);

            _unitOfWork.DidNotReceive().BeginTransaction();
            await _sessionRepository.DidNotReceiveWithAnyArgs()
                .GetAsync(default!, default);
        }

        [Fact]
        public async Task Handle_ShouldThrowNotFoundExceptionAndRollback_WhenSessionDoesNotExist()
        {
            _sessionRepository
                .GetAsync(Arg.Any<GetSessionsByJwtSpecification>(), Arg.Any<CancellationToken>())
                .Returns((SessionEntity?)null);

            var command = new LogoutCommand("some-token", Guid.NewGuid());

            Task act() => _handler.Handle(command, CancellationToken.None);

            var exception = await Assert.ThrowsAsync<NotFoundException>(act);
            Assert.Equal("Session was not found", exception.Message);

            _transaction.Received(1).Rollback();
            _transaction.DidNotReceive().Commit();
            _permissionCacheService.DidNotReceiveWithAnyArgs().RemovePermissions(default);

            // Blacklisting should never be reached
            await _blacklistTokenService.DidNotReceiveWithAnyArgs()
                .BlacklistTokenAsync(default!, default, default);
        }

        [Fact]
        public async Task Handle_ShouldRollbackAndRethrow_WhenBlacklistingFails()
        {
            var session = CreateActiveSession();

            _sessionRepository
                .GetAsync(Arg.Any<GetSessionsByJwtSpecification>(), Arg.Any<CancellationToken>())
                .Returns(session);

            _blacklistTokenService
                .BlacklistTokenAsync(Arg.Any<string>(), Arg.Any<DateTime>(), Arg.Any<CancellationToken>())
                .ThrowsAsync(new InvalidOperationException("Blacklist service unavailable"));

            var command = new LogoutCommand("some-token", Guid.NewGuid());

            Task act() => _handler.Handle(command, CancellationToken.None);

            await Assert.ThrowsAsync<InvalidOperationException>(act);

            _transaction.Received(1).Rollback();
            _transaction.DidNotReceive().Commit();
            _permissionCacheService.DidNotReceiveWithAnyArgs().RemovePermissions(default);
        }

        private static SessionEntity CreateActiveSession()
        {
            return new SessionEntity
            {
                Id = Guid.NewGuid(),
                Jwt = "current-jwt-value",
                RefreshToken = "refresh-token-value",
                Device = "Test Device",
                Browser = "Test Browser",
                IpAddress = "127.0.0.1",
                Location = "Unknown",
                StartTime = DateTime.UtcNow.AddHours(-1),
                LastActivityTime = DateTime.UtcNow,
                ExpiresAt = DateTime.UtcNow.AddDays(7),
                StaffId = Guid.NewGuid(),
                IsActive = true
            };
        }
    }
}