using MediatR;
using NettoRetail.DataAccess.Specifications.Notification;
using NettoRetail.Shared.Interfaces.Repositories;

namespace NettoRetail.Application.CQRS.Notifications.Command;

public record ReadAllNotificationsCommand(Guid StaffId) : IRequest;

public class ReadAllNotificationsCommandHandler(INotificationRepository notificationRepository) : IRequestHandler<ReadAllNotificationsCommand>
{
    public async Task Handle(ReadAllNotificationsCommand request, CancellationToken cancellationToken)
    {
        var specification = new GetUnreadNotificationsByStaffIdSpecification(request.StaffId);
        var notifications = await notificationRepository.GetAllBySpecAsync(specification, cancellationToken);
        if (!notifications.Any())
        {
            return;
        }

        foreach (var notification in notifications)
        {
            notification.IsRead = true;
        }

        await notificationRepository.UpdateManyAsync(notifications, cancellationToken);
    }
}