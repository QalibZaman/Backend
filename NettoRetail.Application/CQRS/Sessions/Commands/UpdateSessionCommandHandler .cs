using MediatR;
using NettoRetail.Application.Exceptions;
using NettoRetail.Application.Models.Session;
using NettoRetail.Application.Services.PermissionCache;
using NettoRetail.DataAccess.Specifications.Sessions;
using NettoRetail.Shared.Interfaces.Repositories;
using NettoRetail.Shared.Utilities;

namespace NettoRetail.Application.CQRS.Sessions.Commands;

public record UpdateSessionCommand(SessionUpdateModel Model) : IRequest;

public class UpdateSessionCommandHandler(ISessionRepository sessionRepository, IPermissionCacheService permissionCacheService) : IRequestHandler<UpdateSessionCommand>
{
    public async Task Handle(UpdateSessionCommand request, CancellationToken cancellationToken)
    {
        if (request.Model.PreviousRefreshToken == null || request.Model.CurrentRefreshToken == null)
            throw new InvalidRefreshTokenException("Failed to update session");

        var expiresAt = JwtUtils.GetJwtExpiration(request.Model.CurrentJwt) ?? throw new InvalidTokenCredentialsException("Invalid JWT or missing expiration claim.");
        var specification = new GetSessionsByRefreshTokenSpecification(request.Model.PreviousRefreshToken);
        var session = await sessionRepository.UpdateAsync(specification, request.Model.CurrentJwt, request.Model.CurrentRefreshToken, request.Model.LastActivityTime, expiresAt, cancellationToken)
                        ?? throw new NotFoundException("Session not found");

        var permissions = permissionCacheService.GetPermissions(session.Id);
        permissionCacheService.RemovePermissions(session.Id);
        permissionCacheService.SetPermissions(permissions!, session.Id);
    }
}