using MediatR;
using Microsoft.Extensions.Primitives;
using NettoRetail.Application.Exceptions;
using NettoRetail.DataAccess.Specifications.Sessions;
using NettoRetail.Shared.Interfaces.Repositories;

namespace NettoRetail.Application.CQRS.Sessions.Commands;

public record TerminateOtherSessionsCommand(Guid Id, StringValues AuthHeader) : IRequest;
public class TerminateOtherSessionsCommandHandler(ISessionRepository sessionRepository) : IRequestHandler<TerminateOtherSessionsCommand>
{
    public async Task Handle(TerminateOtherSessionsCommand request, CancellationToken cancellationToken)
    {

        var currentSessionToken = request.AuthHeader.ToString().Split(" ")[1]
            ?? throw new NotFoundException("Current session is unavailable or not found");

        var specification = new GetSessionsByAccountIdExceptCurrentSpecification(request.Id, currentSessionToken);

        var sessions = await sessionRepository.GetAllBySpecAsync(specification, cancellationToken)
            ?? throw new NotFoundException("Sessions are unavailable or not found");

        foreach (var session in sessions)
        {
            session.IsActive = false;
        }

        await sessionRepository.UpdateManyAsync(sessions, cancellationToken);
    }
}
