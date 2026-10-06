using NettoRetail.Application.CQRS.Notifications.Command;
using NettoRetail.DataAccess.Specifications.Notification;
using NettoRetail.Domain.Entities;
using NettoRetail.Shared.Interfaces.Repositories;
using NSubstitute;
using Xunit;

namespace NettoRetail.Tests.UnitTests.NotificationsTests
{
    public class ReadAllNotificationsCommandHandlerTests
    {
        private readonly INotificationRepository _notificationRepository;
        private readonly ReadAllNotificationsCommandHandler _handler;

        public ReadAllNotificationsCommandHandlerTests()
        {
            _notificationRepository = Substitute.For<INotificationRepository>();
            _handler = new ReadAllNotificationsCommandHandler(_notificationRepository);
        }

        [Fact]
        public async Task Handle_ShouldDoNothing_WhenThereAreNoUnreadNotifications()
        {
            // Arrange
            _notificationRepository
                .GetAllBySpecAsync(Arg.Any<GetUnreadNotificationsByStaffIdSpecification>(), Arg.Any<CancellationToken>())
                .Returns(Enumerable.Empty<NotificationEntity>());

            var command = new ReadAllNotificationsCommand(Guid.NewGuid());

            // Act
            await _handler.Handle(command, CancellationToken.None);

            // Assert
            await _notificationRepository.DidNotReceiveWithAnyArgs()
                .UpdateManyAsync(default!, default);
        }

        [Fact]
        public async Task Handle_ShouldMarkAllUnreadNotificationsAsRead_WhenUnreadNotificationsExist()
        {
            // Arrange
            var staffId = Guid.NewGuid();
            var notifications = new[]
            {
                CreateNotification(staffId),
                CreateNotification(staffId),
                CreateNotification(staffId)
            };

            _notificationRepository
                .GetAllBySpecAsync(Arg.Any<GetUnreadNotificationsByStaffIdSpecification>(), Arg.Any<CancellationToken>())
                .Returns(notifications);

            var command = new ReadAllNotificationsCommand(staffId);

            // Act
            await _handler.Handle(command, CancellationToken.None);

            // Assert
            Assert.All(notifications, n => Assert.True(n.IsRead));

            await _notificationRepository.Received(1)
                .UpdateManyAsync(
                    Arg.Is<IEnumerable<NotificationEntity>>(list => list.SequenceEqual(notifications)),
                    Arg.Any<CancellationToken>());
        }

        private static NotificationEntity CreateNotification(Guid staffId)
        {
            return new NotificationEntity
            {
                Id = Guid.NewGuid(),
                StaffId = staffId,
                CreatedAt = DateTime.UtcNow,
                Message = "Test message",
                Topic = "Test topic",
                IsRead = false
            };
        }
    }
}