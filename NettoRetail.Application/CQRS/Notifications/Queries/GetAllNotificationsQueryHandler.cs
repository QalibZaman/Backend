using MapsterMapper;
using MediatR;
using NettoRetail.Application.Models.Response;
using NettoRetail.DataAccess.Specifications.Notification;
using NettoRetail.Shared.Interfaces.Repositories;

namespace NettoRetail.Application.CQRS.Notifications.Queries;

public record GetAllNotificationsQuery(Guid StaffId) : IRequest<PaginatedResponse<NotificationResponse>>;

public class GetAllNotificationsQueryHandler(INotificationRepository notificationRepository, IMapper mapper) : IRequestHandler<GetAllNotificationsQuery, PaginatedResponse<NotificationResponse>>
{
    public async Task<PaginatedResponse<NotificationResponse>> Handle(GetAllNotificationsQuery request, CancellationToken cancellationToken)
    {
        var specification = new GetNotificationsByStaffIdSpecification(request.StaffId);
        var items = await notificationRepository.GetAllBySpecAsync(specification, cancellationToken);
        var notifications = mapper.Map<ICollection<NotificationResponse>>(items);

        return new PaginatedResponse<NotificationResponse>
        {
            Items = notifications,
            TotalAmount = notifications.Count
        };
    }
}