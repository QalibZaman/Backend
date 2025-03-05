using Mapster;
using MediatR;
using NettoRetail.DataAccess.Specifications;
using NettoRetail.Domain.Entities;
using NettoRetail.Shared.Interfaces.Repositories;

namespace NettoRetail.Application.CQRS.Sessions.Commands;

public record DeleteSessionCommand(string SessionId) : IRequest;

public class DeleteSessionCommandHandler(ISessionRepository sessionRepository)
    : IRequestHandler<DeleteSessionCommand>
{
    public async Task Handle(DeleteSessionCommand request, CancellationToken cancellationToken)
    {
        var specification = new GetSessionsByIdSpecification(request.SessionId);

        await sessionRepository.DeleteAsync(specification, cancellationToken);
    }
}