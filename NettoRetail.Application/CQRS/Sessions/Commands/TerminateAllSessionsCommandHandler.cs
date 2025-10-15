using MediatR;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Primitives;
using NettoRetail.Application.Exceptions;
using NettoRetail.DataAccess.Specifications.Sessions;
using NettoRetail.Domain.Entities;
using NettoRetail.Shared.Interfaces.Repositories;
using NettoRetail.Shared.Specifications;

namespace NettoRetail.Application.CQRS.Sessions.Commands;

public record TerminateAllSessionsCommand(Guid Id, StringValues AuthHeader) : IRequest;
public class TerminateAllSessionsCommandHandler(ISessionRepository sessionRepository) : IRequestHandler<TerminateAllSessionsCommand>
{
    public async Task Handle(TerminateAllSessionsCommand request, CancellationToken cancellationToken)
    {

        var currentSessionToken = request.AuthHeader.ToString().Split(" ")[1]
            ?? throw new NotFoundException("Session unavailable or not found");

        var specification = new GetSessionsByAccountIdExceptCurrentSpecification(request.Id, currentSessionToken);

        var sessions = await sessionRepository.GetAllBySpecAsync(specification, cancellationToken);

        foreach (var session in sessions)
        {
            session.IsActive = false;
        }

        await sessionRepository.UpdateManyAsync(sessions, cancellationToken);
    }
}
