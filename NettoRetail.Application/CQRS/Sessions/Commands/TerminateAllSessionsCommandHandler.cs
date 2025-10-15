using MediatR;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Primitives;
using NettoRetail.Application.Exceptions;
using NettoRetail.DataAccess.Specifications.Sessions;
using NettoRetail.Shared.Interfaces.Repositories;

namespace NettoRetail.Application.CQRS.Sessions.Commands;

public record TerminateAllSessionsCommand(Guid Id, StringValues AuthHeader) : IRequest;
public class TerminateAllSessionsCommandHandler(ISessionRepository sessionRepository) : IRequestHandler<TerminateAllSessionsCommand>
{
    public async Task Handle(TerminateAllSessionsCommand request, CancellationToken cancellationToken)
    {
        var specification = new GetSessionsByAccountIdSpecification(request.Id.ToString());

        var sessions = await sessionRepository.GetAllBySpecAsync(specification, cancellationToken);

        var token = request.AuthHeader.ToString().Split(" ")[1]
            ?? throw new NotFoundException("Session unavailable or not found");

        foreach (var session in sessions.Where(s => s.Jwt.ToString() != token.ToString()
            && s.IsActive))
        {
            session.IsActive = false;
        }

        await sessionRepository.UpdateManyAsync(sessions, cancellationToken);
    }
}
