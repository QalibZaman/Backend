using Bogus.DataSets;
using MediatR;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Primitives;
using NettoRetail.Application.Exceptions;
using NettoRetail.Application.Models.Session;
using NettoRetail.DataAccess.Specifications.Sessions;
using NettoRetail.Shared.Interfaces.Repositories;
using NettoRetail.Shared.Utilities;

namespace NettoRetail.Application.CQRS.Sessions.Commands;

public record TerminateSessionCommand(Guid Id, Guid UserId, string CurrentSessionToken) : IRequest;
public class TerminateSessionCommandHandler(ISessionRepository sessionRepository) : IRequestHandler<TerminateSessionCommand>
{
    public async Task Handle(TerminateSessionCommand request, CancellationToken cancellationToken)
    {
        var specification = new GetSessionByIdAndAccountIdSpecification(request.Id, request.UserId);

        var session = await sessionRepository.GetAsync(specification, cancellationToken)
            ?? throw new NotFoundException("Session is unavailable or not found");

        if (session.Jwt.ToString() == request.CurrentSessionToken)
        {
            throw new ConflictException("Can't terminate current session");
        }

        session.IsActive = false;

        await sessionRepository.UpdateAsync(session, cancellationToken);
    }

}
