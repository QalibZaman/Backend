using MapsterMapper;
using MediatR;
using NettoRetail.Application.Models.Response;
using NettoRetail.DataAccess.Specifications.NotificationType;
using NettoRetail.Domain.Entities;
using NettoRetail.Shared.Interfaces.Repositories;

namespace NettoRetail.Application.CQRS.Notifications.Queries;

public record GetAllNotificationPreferencesQuery(Guid StaffId) : IRequest<List<NotificationPreferenceResponse>>;

public class GetAllNotificationPreferencesQueryHandler(
    INotificationPreferenceRepository repository,
    INotificationTypeRepository typeRepository,
    IMapper mapper)
    : IRequestHandler<GetAllNotificationPreferencesQuery, List<NotificationPreferenceResponse>>
{
    public async Task<List<NotificationPreferenceResponse>> Handle(GetAllNotificationPreferencesQuery request, CancellationToken cancellationToken)
    {
        var (preferences, totalCount) =
            await repository.GetAllPaginatedAsync(
                new GetNotificationPreferenceByStaffId(request.StaffId),
                cancellationToken);
        var types = await typeRepository.GetAllAsync(cancellationToken);

        foreach (var type in types)
        {
            var exists = preferences.Any(x => x.NotificationTypeId == type.Id);

            if (exists)
                continue;

            preferences.Add(new NotificationPreferenceEntity
            {
                StaffId = request.StaffId,
                NotificationTypeId = type.Id,
                NotificationType = type,
                IsEnabled = true
            });
        }

        var response = mapper.Map<List<NotificationPreferenceResponse>>(preferences);
        return response;
    }
}