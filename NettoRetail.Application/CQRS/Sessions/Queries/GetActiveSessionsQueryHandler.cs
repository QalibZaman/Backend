using Mapster;
using MediatR;
using NettoRetail.Application.Models.Response;
using NettoRetail.DataAccess.Specifications;
using NettoRetail.Shared.Interfaces.Repositories;

namespace NettoRetail.Application.CQRS.Sessions.Queries;

public record GetActiveSessionsQuery(Guid AccountId) : IRequest<IEnumerable<SessionResponse>>;

public class GetActiveSessionsQueryHandler(ISessionRepository sessionRepository) : IRequestHandler<GetActiveSessionsQuery, IEnumerable<SessionResponse>>
{
    public async Task<IEnumerable<SessionResponse>> Handle(GetActiveSessionsQuery request, CancellationToken cancellationToken)
    {
        var specification = new GetSessionsByIdSpecification(request.AccountId.ToString());

        var activeSessions = await sessionRepository.GetManyBySpecAsync(specification, cancellationToken);
        var sessionResponses = activeSessions.Adapt<IEnumerable<SessionResponse>>();

        return sessionResponses;
    }
}
