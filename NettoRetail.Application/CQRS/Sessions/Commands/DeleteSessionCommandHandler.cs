using Mapster;
using MediatR;
using NettoRetail.DataAccess.Specifications;
using NettoRetail.Domain.Entities;
using NettoRetail.Shared.Interfaces.Repositories;

namespace NettoRetail.Application.CQRS.Sessions.Commands;

public record DeleteSessionCommand(string sessionId) : IRequest;

public class DeleteSessionCommandHandler(ISessionRepository sessionRepository)
    : IRequestHandler<DeleteSessionCommand>
{
    public async Task Handle(DeleteSessionCommand request, CancellationToken cancellationToken)
    {
        var specification = new GetSessionsByIdSpecification(request.sessionId);

        await sessionRepository.DeleteAsync(specification, cancellationToken);
    }
}