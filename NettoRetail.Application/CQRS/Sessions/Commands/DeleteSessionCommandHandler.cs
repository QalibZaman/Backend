using MediatR;
using NettoRetail.DataAccess.Specifications.Sessions;
using NettoRetail.Shared.Interfaces.Repositories;

namespace NettoRetail.Application.CQRS.Sessions.Commands;

public record DeleteSessionCommand(string Jwt) : IRequest;

public class DeleteSessionCommandHandler(ISessionRepository sessionRepository)
    : IRequestHandler<DeleteSessionCommand>
{
    public async Task Handle(DeleteSessionCommand request, CancellationToken cancellationToken)
    {
        var specification = new GetSessionsByJwtSpecification(request.Jwt);

        await sessionRepository.DeleteAsync(specification, cancellationToken);
    }
}