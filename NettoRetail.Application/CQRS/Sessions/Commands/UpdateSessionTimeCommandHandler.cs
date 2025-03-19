using MediatR;
using NettoRetail.Application.Exceptions;
using NettoRetail.Application.Models.Session;
using NettoRetail.DataAccess.Specifications.Sessions;
using NettoRetail.Shared.Interfaces.Repositories;
using NettoRetail.Shared.Utilities;

namespace NettoRetail.Application.CQRS.Sessions.Commands;

public record UpdateSessionTimeCommand(SessionUpdateModel Model) : IRequest;

public class UpdateSessionTimeCommandHandler(ISessionRepository sessionRepository) : IRequestHandler<UpdateSessionTimeCommand>
{
    public async Task Handle(UpdateSessionTimeCommand request, CancellationToken cancellationToken)
    {
        var expiresAt = JwtUtils.GetJwtExpiration(request.Model.CurrentJwt) ?? throw new InvalidTokenCredentialsException("Invalid JWT or missing expiration claim.");
        var specification = new GetSessionsByJwtSpecification(request.Model.CurrentJwt);
        await sessionRepository.UpdateAsync(specification, request.Model.CurrentJwt, null, request.Model.LastActivityTime, expiresAt, cancellationToken);
    }
}