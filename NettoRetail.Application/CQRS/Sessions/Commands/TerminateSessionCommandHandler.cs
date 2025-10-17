using Bogus.DataSets;
using MediatR;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Primitives;
using NettoRetail.Application.Exceptions;
using NettoRetail.Application.Models.Session;
using NettoRetail.DataAccess.Specifications.Sessions;
using NettoRetail.Shared.Interfaces.Repositories;

namespace NettoRetail.Application.CQRS.Sessions.Commands;

public record TerminateSessionCommand(SessionTerminateModel Model, StringValues AuthHeader) : IRequest;
public class TerminateSessionCommandHandler(ISessionRepository sessionRepository) : IRequestHandler<TerminateSessionCommand>
{
    public async Task Handle(TerminateSessionCommand request, CancellationToken cancellationToken)
    {
        var specification = new GetSessionByIdSpecification(request.Model.Id);

        var session = await sessionRepository.GetAsync(specification, cancellationToken)
            ?? throw new NotFoundException("Session is unavailable or not found");

        var currentSessionToken = request.AuthHeader.ToString().Split(" ")[1]
            ?? throw new NotFoundException("Current session is unavailable or not found");

        if (session.Jwt.ToString() == currentSessionToken)
        {
            throw new ConflictException("Can't terminate current session");
        }

        session.IsActive = false;

        await sessionRepository.UpdateAsync(session, cancellationToken);
    }

}
