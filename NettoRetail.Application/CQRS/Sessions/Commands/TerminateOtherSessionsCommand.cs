using MediatR;
using NettoRetail.DataAccess.Specifications.Sessions;
using NettoRetail.Shared.Interfaces.Repositories;

namespace NettoRetail.Application.CQRS.Sessions.Commands
{
    public record TerminateOtherSessionsCommand(Guid Id, string CurrentSessionToken) : IRequest;
    public class TerminateOtherSessionsCommandHandler(ISessionRepository sessionRepository) : IRequestHandler<TerminateOtherSessionsCommand>
    {
        public async Task Handle(TerminateOtherSessionsCommand request, CancellationToken cancellationToken)
        {
            var specification = new GetSessionsByAccountIdExceptCurrentSpecification(request.Id, request.CurrentSessionToken);

            await sessionRepository.TerminateSessionsAsync(specification, cancellationToken);
        }
    }
}
