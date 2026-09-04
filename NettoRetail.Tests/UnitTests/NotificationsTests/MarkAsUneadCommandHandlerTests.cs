using NettoRetail.Application.CQRS.Notifications.Command;
using NettoRetail.Application.Exceptions;
using NettoRetail.DataAccess.Specifications.Notification;
using NettoRetail.Domain.Entities;
using NettoRetail.Shared.Interfaces.Repositories;
using NSubstitute;
using Xunit;

namespace NettoRetail.Tests.UnitTests.NotificationsTests
{
    public class MarkAsUneadCommandHandlerTests
    {
        private readonly INotificationRepository _notificationRepository;
        private readonly MarkAsUneadCommandHandler _handler;

        public MarkAsUneadCommandHandlerTests()
        {
            _notificationRepository = Substitute.For<INotificationRepository>();
            _handler = new MarkAsUneadCommandHandler(_notificationRepository);
        }

        [Fact]
        public async Task Handle_ShouldThrowNotFoundException_WhenNotificationDoesNotExist()
        {
            // Arrange
            _notificationRepository
                .GetAsync(Arg.Any<GetNotificationByIdAndStaffIdSpecification>(), Arg.Any<CancellationToken>())
                .Returns((NotificationEntity?)null);

            var command = new MarkAsUneadCommand(Guid.NewGuid(), Guid.NewGuid());

            // Act
            var act = () => _handler.Handle(command, CancellationToken.None);

            // Assert
            var exception = await Assert.ThrowsAsync<NotFoundException>(act);
            Assert.Equal("Notification not found", exception.Message);

            await _notificationRepository.DidNotReceiveWithAnyArgs()
                .UpdateAsync(default!, default);
        }

        [Fact]
        public async Task Handle_ShouldMarkNotificationAsUnread_WhenItWasRead()
        {
            // Arrange
            var notification = CreateNotification(isRead: true);

            _notificationRepository
                .GetAsync(Arg.Any<GetNotificationByIdAndStaffIdSpecification>(), Arg.Any<CancellationToken>())
                .Returns(notification);

            var command = new MarkAsUneadCommand(notification.Id, notification.StaffId);

            // Act
            await _handler.Handle(command, CancellationToken.None);

            // Assert
            Assert.False(notification.IsRead);
            await _notificationRepository.Received(1).UpdateAsync(notification, Arg.Any<CancellationToken>());
        }

        [Fact]
        public async Task Handle_ShouldNotUpdate_WhenNotificationWasAlreadyUnread()
        {
            // Arrange - marking an already-unread notification again should be a no-op
            var notification = CreateNotification(isRead: false);

            _notificationRepository
                .GetAsync(Arg.Any<GetNotificationByIdAndStaffIdSpecification>(), Arg.Any<CancellationToken>())
                .Returns(notification);

            var command = new MarkAsUneadCommand(notification.Id, notification.StaffId);

            // Act
            await _handler.Handle(command, CancellationToken.None);

            // Assert
            Assert.False(notification.IsRead);
            await _notificationRepository.DidNotReceiveWithAnyArgs()
                .UpdateAsync(default!, default);
        }

        private static NotificationEntity CreateNotification(bool isRead)
        {
            return new NotificationEntity
            {
                Id = Guid.NewGuid(),
                StaffId = Guid.NewGuid(),
                CreatedAt = DateTime.UtcNow,
                Message = "Test message",
                Topic = "Test topic",
                IsRead = isRead
            };
        }
    }
}