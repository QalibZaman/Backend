using MediatR;
using NettoRetail.Application.Exceptions;
using NettoRetail.DataAccess.Specifications.Notification;
using NettoRetail.Shared.Interfaces.Repositories;

namespace NettoRetail.Application.CQRS.Notifications.Command;

public record MarkAsReadCommand(Guid Id, Guid StaffId) : IRequest;

public class MarkAsReadCommandHandler(INotificationRepository notificationRepository): IRequestHandler<MarkAsReadCommand>
{
    public async Task Handle(MarkAsReadCommand request, CancellationToken cancellationToken)
    {
        var specification = new GetNotificationByIdAndStaffIdSpecification(request.Id, request.StaffId);
        var notification = await notificationRepository.GetAsync(specification, cancellationToken);
        if (notification == null)
        {
            throw new NotFoundException("Notification not found");
        }

        if (!notification.IsRead)
        {
            notification.IsRead = true;
            await notificationRepository.UpdateAsync(notification, cancellationToken);
        }
    }
}