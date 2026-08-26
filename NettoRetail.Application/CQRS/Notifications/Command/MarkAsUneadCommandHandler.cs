using MediatR;
using NettoRetail.Application.Exceptions;
using NettoRetail.DataAccess.Specifications.Notification;
using NettoRetail.Shared.Interfaces.Repositories;

namespace NettoRetail.Application.CQRS.Notifications.Command;

public record MarkAsUneadCommand(Guid Id, Guid StaffId) : IRequest;

public class MarkAsUneadCommandHandler(INotificationRepository notificationRepository): IRequestHandler<MarkAsUneadCommand>
{
    public async Task Handle(MarkAsUneadCommand request, CancellationToken cancellationToken)
    {
        var specification = new GetNotificationByIdAndStaffIdSpecification(request.Id, request.StaffId);
        var notification = await notificationRepository.GetAsync(specification, cancellationToken);
        if (notification == null)
        {
            throw new NotFoundException("Notification not found");
        }

        if (notification.IsRead)
        {
            notification.IsRead = false;
            await notificationRepository.UpdateAsync(notification, cancellationToken);
        }
    }
}