using MediatR;
using NettoRetail.Application.Exceptions;
using NettoRetail.Application.Models.Request.Notification;
using NettoRetail.DataAccess.Specifications.NotificationType;
using NettoRetail.DataAccess.Specifications.Staff;
using NettoRetail.Domain.Entities;
using NettoRetail.Shared.Interfaces.Repositories;

namespace NettoRetail.Application.CQRS.Notifications.Command;

public record SetNotificationPreferenceCommand(UpdateNotificationPreferenceRequest Model, Guid StaffId) : IRequest;

public class SetNotificationPreferenceCommandHandler(
    INotificationPreferenceRepository repository,
    IStaffRepository staffRepository, INotificationTypeRepository notificationTypeRepository) : IRequestHandler<SetNotificationPreferenceCommand>
{
    public async Task Handle(SetNotificationPreferenceCommand request, CancellationToken cancellationToken)
    {
        if (!await staffRepository.ExistsAsync(
                new GetStaffByIdSpecification(request.StaffId),
                cancellationToken))
        {
            throw new NotFoundException("Staff not found");
        }
        var preference =
            await repository.GetAsync(
                new GetNotificationPreferenceByStaffIdAndTypeId(request.StaffId, request.Model.NotificationTypeId),
                cancellationToken);
        var notificationType = await notificationTypeRepository.GetAsync(
            new GetNotificationTypeByIdSpecification(request.Model.NotificationTypeId),
            cancellationToken);

        if (notificationType == null)
            throw new NotFoundException("Notification type not found");

        if (preference == null)
        {
            preference = new NotificationPreferenceEntity
            {
                NotificationTypeId = request.Model.NotificationTypeId,
                StaffId = request.StaffId,
                IsEnabled = request.Model.IsEnabled
            };
            await repository.AddAsync(preference, cancellationToken);
        }
        else
        {
            preference.IsEnabled = request.Model.IsEnabled;
            await repository.UpdateAsync(preference, cancellationToken);
        }
    }
}