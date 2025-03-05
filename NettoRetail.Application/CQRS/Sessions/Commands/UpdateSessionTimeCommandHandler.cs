using MediatR;
using NettoRetail.Application.Models.Session;
using NettoRetail.DataAccess.Specifications;
using NettoRetail.Shared.Interfaces.Repositories;

namespace NettoRetail.Application.CQRS.Sessions.Commands;

public record UpdateSessionTimeCommand(SessionUpdateModel Model) : IRequest;

public class UpdateSessionTimeCommandHandler(ISessionRepository sessionRepository) : IRequestHandler<UpdateSessionTimeCommand>
{
    public async Task Handle(UpdateSessionTimeCommand request, CancellationToken cancellationToken)
    {
        var specification = new GetSessionsByJwtSpecification(request.Model.CurrentJwt);
        await sessionRepository.UpdateAsync(specification, request.Model.CurrentJwt, request.Model.LastActivityTime, cancellationToken);
    }
}