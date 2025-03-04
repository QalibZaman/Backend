using Mapster;
using MediatR;
using NettoRetail.Application.Models.Response;
using NettoRetail.DataAccess.Specifications;
using NettoRetail.Shared.Interfaces.Repositories;

namespace NettoRetail.Application.CQRS.Sessions.Queries;

public record GetActiveSessionsQuery(string Jwt) : IRequest<IEnumerable<SessionResponse>>;

public class GetActiveSessionsQueryHandler : IRequestHandler<GetActiveSessionsQuery, IEnumerable<SessionResponse>>
{
    private readonly ISessionRepository _sessionRepository;

    public GetActiveSessionsQueryHandler(ISessionRepository sessionRepository)
    {
        _sessionRepository = sessionRepository;
    }

    public async Task<IEnumerable<SessionResponse>> Handle(GetActiveSessionsQuery request, CancellationToken cancellationToken)
    {
        var specification = new GetSessionsByJwtSpecification(request.Jwt);

        var activeSessions = await _sessionRepository.GetManyBySpecAsync(specification, cancellationToken);
        var sessionResponses = activeSessions.Adapt<IEnumerable<SessionResponse>>();

        return sessionResponses;
    }
}
