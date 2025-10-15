using MediatR;
using NettoRetail.Application.Exceptions;
using NettoRetail.Application.Models.Session;
using NettoRetail.DataAccess.Specifications.Sessions;
using NettoRetail.Shared.Interfaces.Repositories;

namespace NettoRetail.Application.CQRS.Sessions.Commands;

public record TerminateSessionCommand(SessionTerminateModel Model) : IRequest;
public class TerminateSessionCommandHandler(ISessionRepository sessionRepository) : IRequestHandler<TerminateSessionCommand>
{
    public async Task Handle(TerminateSessionCommand request, CancellationToken cancellationToken)
    {
        var specification = new GetSessionByIdSpecification(request.Model.Id);

        var session = await sessionRepository.GetAsync(specification, cancellationToken)
            ?? throw new NotFoundException("Session not found");

        session.IsActive = false;

        await sessionRepository.UpdateAsync(session, cancellationToken);
    }

}
