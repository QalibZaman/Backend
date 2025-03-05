using MediatR;
using NettoRetail.Application.Exceptions;
using NettoRetail.Application.Models.Session;
using NettoRetail.Application.Services.ISessionModelFactory;
using NettoRetail.DataAccess.Specifications;
using NettoRetail.Infrastructure.Interfaces;
using NettoRetail.Shared.Interfaces.Repositories;

namespace NettoRetail.Application.CQRS.Sessions.Commands;

public record UpdateSessionCommand(SessionUpdateModel Model) : IRequest;

public class UpdateSessionCommandHandler(ISessionRepository sessionRepository) : IRequestHandler<UpdateSessionCommand>
{
    public async Task Handle(UpdateSessionCommand request, CancellationToken cancellationToken)
    {
        if (request.Model.PreviousRefreshToken == null)
            throw new InvalidRefreshTokenException("Failed to update session");

        var specification = new GetSessionsByRefreshTokenSpecification(request.Model.PreviousRefreshToken);
        await sessionRepository.UpdateAsync(specification, request.Model.CurrentJwt, request.Model.LastActivityTime, cancellationToken);
    }
}