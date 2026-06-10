using FluentAssertions;
using NettoRetail.Application.CQRS.Users.Commands.Staff;
using NettoRetail.Application.Exceptions;
using NettoRetail.Application.Interfaces;
using NettoRetail.Application.Services.INotificationService;
using NettoRetail.DataAccess.Specifications.Staff;
using NettoRetail.Domain.Entities;
using NettoRetail.Shared.Constants;
using NettoRetail.Shared.Interfaces.Repositories;
using NettoRetail.Shared.Interfaces.Utilities;
using NSubstitute;
using NSubstitute.ExceptionExtensions;
using StackExchange.Redis;
using System.Data;
namespace NettoRetail.Tests.UnitTests.UsersTests.StaffTests
{
    public class DeleteStaffCommandHandlerTests
    {
        private readonly IStaffRepository _staffRepository;
        private readonly ISessionTerminationService _sessionTerminationService;
        private readonly IUnitOfWork _unitOfWork;
        private readonly IDbTransaction _transaction;
        private readonly INotificationService _notificationService;
        private readonly DeleteStaffCommandHandler _handler;

        public DeleteStaffCommandHandlerTests()
        {
            _staffRepository = Substitute.For<IStaffRepository>();
            _sessionTerminationService = Substitute.For<ISessionTerminationService>();
            _unitOfWork = Substitute.For<IUnitOfWork>();
            _transaction = Substitute.For<IDbTransaction>();
            _notificationService = Substitute.For<INotificationService>();

            _unitOfWork.BeginTransaction().Returns(_transaction);

            _handler = new DeleteStaffCommandHandler(
                _staffRepository,
                _sessionTerminationService,
                _unitOfWork,
                _notificationService);
        }

        [Fact]
        public async Task Handle_ShouldDeleteStaff_WhenAccountExists()
        {
            // Arrange
            var command = new DeleteStaffCommand(Guid.NewGuid(), 1);
            var account = new StaffEntity
            {
                Id = command.Id,
                ShopId = command.ShopId,
                FirstName = "Test",
                LastName = "User",
                Email = "test@example.com",
                PasswordHash = "hash",
                Sessions = new List<SessionEntity>()
            };

            _staffRepository
                .GetAsync(Arg.Any<GetStaffByIdSpecification>(), Arg.Any<CancellationToken>())
                .Returns(account);

            // Act
            await _handler.Handle(command, CancellationToken.None);

            // Assert
            await _staffRepository.Received(1).DeleteAsync(account, Arg.Any<CancellationToken>());
            _transaction.Received(1).Commit();
        }

        [Fact]
        public async Task Handle_ShouldThrowNotFoundException_WhenAccountDoesNotExist()
        {
            // Arrange
            var command = new DeleteStaffCommand(Guid.NewGuid(), 1);

            _staffRepository
                .GetAsync(Arg.Any<GetStaffByIdSpecification>(), Arg.Any<CancellationToken>())
                .Returns((StaffEntity?)null);

            // Act
            var act = async () => await _handler.Handle(command, CancellationToken.None);

            // Assert
            await act.Should().ThrowAsync<NotFoundException>()
                .WithMessage(StaffConstants.StaffMemberNotFound);
        }

        [Fact]
        public async Task Handle_ShouldThrowForbiddenException_WhenAccountBelongsToAnotherShop()
        {
            // Arrange
            var command = new DeleteStaffCommand(Guid.NewGuid(), 1);

            _staffRepository
                .GetAsync(Arg.Any<GetStaffByIdSpecification>(), Arg.Any<CancellationToken>())
                .Returns(new StaffEntity
                {
                    Id = command.Id,
                    ShopId = 2,
                    FirstName = "Other",
                    LastName = "User",
                    Email = "other@example.com",
                    PasswordHash = "hash",
                    Sessions = new List<SessionEntity>()
                });

            // Act
            var act = async () => await _handler.Handle(command, CancellationToken.None);

            // Assert
            await act.Should().ThrowAsync<ForbiddenException>()
                .WithMessage(StaffConstants.StaffMemberAccessDenied);
        }

        [Fact]
        public async Task Handle_ShouldSkipSessionTermination_WhenAccountIsOffline()
        {
            // Arrange
            var command = new DeleteStaffCommand(Guid.NewGuid(), 1);
            var account = new StaffEntity
            {
                Id = command.Id,
                ShopId = command.ShopId,
                FirstName = "Offline",
                LastName = "User",
                Email = "offline@example.com",
                PasswordHash = "hash",
                Sessions = new List<SessionEntity>()
            };

            _staffRepository
                .GetAsync(Arg.Any<GetStaffByIdSpecification>(), Arg.Any<CancellationToken>())
                .Returns(account);

            // Act
            await _handler.Handle(command, CancellationToken.None);

            // Assert
            await _sessionTerminationService
                .DidNotReceive()
                .TerminateSessionsAsync(Arg.Any<Guid>(), Arg.Any<CancellationToken>());
        }

        [Fact]
        public async Task Handle_ShouldSkipNotification_WhenAccountIsOffline()
        {
            // Arrange
            var command = new DeleteStaffCommand(Guid.NewGuid(), 1);
            var account = new StaffEntity
            {
                Id = command.Id,
                ShopId = command.ShopId,
                FirstName = "Offline",
                LastName = "User",
                Email = "offline@example.com",
                PasswordHash = "hash",
                Sessions = new List<SessionEntity>()
            };

            _staffRepository
                .GetAsync(Arg.Any<GetStaffByIdSpecification>(), Arg.Any<CancellationToken>())
                .Returns(account);

            // Act
            await _handler.Handle(command, CancellationToken.None);

            // Assert
            await _notificationService
                .DidNotReceive()
                .NotifyUserAsync(Arg.Any<string>(), Arg.Any<string>());
        }

        [Fact]
        public async Task Handle_ShouldNotCommitTransaction_WhenRepositoryThrowsException()
        {
            // Arrange
            var command = new DeleteStaffCommand(Guid.NewGuid(), 1);
            var account = new StaffEntity
            {
                Id = command.Id,
                ShopId = command.ShopId,
                FirstName = "Test",
                LastName = "User",
                Email = "test@example.com",
                PasswordHash = "hash",
                Sessions = new List<SessionEntity>()
            };

            _staffRepository
                .GetAsync(Arg.Any<GetStaffByIdSpecification>(), Arg.Any<CancellationToken>())
                .Returns(account);

            _staffRepository
                .DeleteAsync(Arg.Any<StaffEntity>(), Arg.Any<CancellationToken>())
                .Throws(new Exception("DB error"));

            // Act
            var act = async () => await _handler.Handle(command, CancellationToken.None);

            // Assert
            await act.Should().ThrowAsync<Exception>();
            _transaction.DidNotReceive().Commit();
        }
    }
}
