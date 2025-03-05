using MediatR;
using NettoRetail.Application.Exceptions;
using NettoRetail.Application.Models.Session;
using NettoRetail.DataAccess.Specifications;
using NettoRetail.Shared.Interfaces.Repositories;

namespace NettoRetail.Application.CQRS.Sessions.Commands;

public record UpdateSessionCommand(SessionUpdateModel Model) : IRequest;

public class UpdateSessionCommandHandler(ISessionRepository sessionRepository) : IRequestHandler<UpdateSessionCommand>
{
    public async Task Handle(UpdateSessionCommand request, CancellationToken cancellationToken)
    {
        if (request.Model.PreviousRefreshToken == null || request.Model.CurrentRefreshToken == null)
            throw new InvalidRefreshTokenException("Failed to update session");

        var specification = new GetSessionsByRefreshTokenSpecification(request.Model.PreviousRefreshToken);
        await sessionRepository.UpdateAsync(specification, request.Model.CurrentJwt, request.Model.CurrentRefreshToken, request.Model.LastActivityTime, cancellationToken);
    }
}