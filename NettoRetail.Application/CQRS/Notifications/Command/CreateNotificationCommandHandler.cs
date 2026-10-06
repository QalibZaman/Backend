using MediatR;
using NettoRetail.Application.Exceptions;
using NettoRetail.Application.Models.Request.Notification;
using NettoRetail.Application.Models.Response;
using NettoRetail.DataAccess.Specifications.Staff;
using NettoRetail.Domain.Entities;
using NettoRetail.Shared.Interfaces.Repositories;

namespace NettoRetail.Application.CQRS.Notifications.Command
{
    public record CreateNotificationCommand(CreateNotificationRequest model) : IRequest<AddNotificationResponce>;
    public class CreateNotificationCommandHandler(
        INotificationRepository notificationRepository,
        IStaffRepository staffRepository) : IRequestHandler<CreateNotificationCommand, AddNotificationResponce>
    {
        public async Task<AddNotificationResponce> Handle(CreateNotificationCommand request, CancellationToken cancellationToken)
        {
            bool staffExists = await staffRepository.ExistsAsync(
                new GetStaffByIdSpecification(request.model.StaffId),
                cancellationToken);
            if (!staffExists)
                throw new NotFoundException($"User with provided Id not found");

            var notification = new NotificationEntity
            {
                StaffId = request.model.StaffId,
                CreatedAt = DateTime.UtcNow,
                Message = request.model.Message,
                IsRead = false
            };

            var newNotification = await notificationRepository.AddAsync(notification, cancellationToken);

            var notificationResponse = new AddNotificationResponce
            {
                Id = newNotification.Id,
                StaffId = newNotification.StaffId,
                Message = newNotification.Message,
                CreatedAt = newNotification.CreatedAt,
                IsRead = false,
                Url = newNotification.Url
            };
            return notificationResponse;
        }
    }
}
