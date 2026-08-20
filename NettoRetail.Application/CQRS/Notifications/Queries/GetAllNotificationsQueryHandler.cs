using MapsterMapper;
using MediatR;
using NettoRetail.Application.Models.Request;
using NettoRetail.Application.Models.Response;
using NettoRetail.Application.Utilities;
using NettoRetail.DataAccess.Specifications.Notification;
using NettoRetail.Shared.Interfaces.Repositories;

namespace NettoRetail.Application.CQRS.Notifications.Queries;

public record GetAllNotificationsQuery(PaginationRequest Model, Guid StaffId) : IRequest<PaginatedResponse<NotificationResponse>>;

public class GetAllNotificationsQueryHandler(INotificationRepository notificationRepository, IMapper mapper) : IRequestHandler<GetAllNotificationsQuery, PaginatedResponse<NotificationResponse>>
{
    public async Task<PaginatedResponse<NotificationResponse>> Handle(GetAllNotificationsQuery request, CancellationToken cancellationToken)
    {
        var skip = PaginationHelper.CalculateSkip(request.Model.PageNumber, request.Model.PageSize);
        var specification = new GetNotificationsByStaffIdSpecificationWithPagination(request.StaffId, skip,  request.Model.PageSize);
        var (items, totalCount) = await notificationRepository.GetAllPaginatedAsync(specification, cancellationToken);
        var notifications = mapper.Map<ICollection<NotificationResponse>>(items);

        return new PaginatedResponse<NotificationResponse>
        {
            Items = notifications,
            TotalAmount = totalCount
        };
    }
}