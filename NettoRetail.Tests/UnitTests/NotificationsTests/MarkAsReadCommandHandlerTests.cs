using NettoRetail.Application.CQRS.Notifications.Command;
using NettoRetail.Application.Exceptions;
using NettoRetail.DataAccess.Specifications.Notification;
using NettoRetail.Domain.Entities;
using NettoRetail.Shared.Interfaces.Repositories;
using NSubstitute;
using Xunit;

namespace NettoRetail.Tests.UnitTests.NotificationsTests
{
    public class MarkAsReadCommandHandlerTests
    {
        private readonly INotificationRepository _notificationRepository;
        private readonly MarkAsReadCommandHandler _handler;

        public MarkAsReadCommandHandlerTests()
        {
            _notificationRepository = Substitute.For<INotificationRepository>();
            _handler = new MarkAsReadCommandHandler(_notificationRepository);
        }

        [Fact]
        public async Task Handle_ShouldThrowNotFoundException_WhenNotificationDoesNotExist()
        {
            // Arrange
            _notificationRepository
                .GetAsync(Arg.Any<GetNotificationByIdAndStaffIdSpecification>(), Arg.Any<CancellationToken>())
                .Returns((NotificationEntity?)null);

            var command = new MarkAsReadCommand(Guid.NewGuid(), Guid.NewGuid());

            // Act
            var act = () => _handler.Handle(command, CancellationToken.None);

            // Assert
            var exception = await Assert.ThrowsAsync<NotFoundException>(act);
            Assert.Equal("Notification not found", exception.Message);

            await _notificationRepository.DidNotReceiveWithAnyArgs()
                .UpdateAsync(default!, default);
        }

        [Fact]
        public async Task Handle_ShouldMarkNotificationAsRead_WhenItWasUnread()
        {
            // Arrange
            var notification = CreateNotification(isRead: false);

            _notificationRepository
                .GetAsync(Arg.Any<GetNotificationByIdAndStaffIdSpecification>(), Arg.Any<CancellationToken>())
                .Returns(notification);

            var command = new MarkAsReadCommand(notification.Id, notification.StaffId);

            // Act
            await _handler.Handle(command, CancellationToken.None);

            // Assert
            Assert.True(notification.IsRead);
            await _notificationRepository.Received(1).UpdateAsync(notification, Arg.Any<CancellationToken>());
        }

        [Fact]
        public async Task Handle_ShouldNotUpdate_WhenNotificationWasAlreadyRead()
        {
            // Arrange - marking an already-read notification again should be a no-op
            var notification = CreateNotification(isRead: true);

            _notificationRepository
                .GetAsync(Arg.Any<GetNotificationByIdAndStaffIdSpecification>(), Arg.Any<CancellationToken>())
                .Returns(notification);

            var command = new MarkAsReadCommand(notification.Id, notification.StaffId);

            // Act
            await _handler.Handle(command, CancellationToken.None);

            // Assert
            Assert.True(notification.IsRead);
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
                IsRead = isRead
            };
        }
    }
}