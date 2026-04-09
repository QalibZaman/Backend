using MapsterMapper;
using MediatR;
using NettoRetail.Application.Exceptions;
using NettoRetail.Application.Models.Response;
using NettoRetail.DataAccess.Specifications.Staff;
using NettoRetail.Shared.Interfaces.Repositories;

namespace NettoRetail.Application.CQRS.Users.Queries.Account;

public record GetCommunicationPreferencesByAccountIdQuery(Guid userId)
    : IRequest<CommunicationPreferencesResponse>;
public class GetCommunicationPreferencesByAccountIdQueryHandler(ICommunicationPreferencesRepository communicationPreferencesRepository, IMapper mapper)
    : IRequestHandler<GetCommunicationPreferencesByAccountIdQuery, CommunicationPreferencesResponse>
{
    public async Task<CommunicationPreferencesResponse> Handle(
        GetCommunicationPreferencesByAccountIdQuery request,
        CancellationToken cancellationToken)
    {
        var specification = new GetCommunicationPreferencesByAccountIdSpecification(request.userId);

        var returnedPreferences = await communicationPreferencesRepository
            .GetAsync(specification, cancellationToken) ?? throw new NotFoundException("Preferences not found");

        var preferencesResponse = mapper.Map<CommunicationPreferencesResponse>(returnedPreferences);

        return preferencesResponse;
    }
}
