using MediatR;
using NettoRetail.Application.Models.Session;
using NettoRetail.Application.Services.ISessionModelFactory;
using NettoRetail.DataAccess.Specifications;
using NettoRetail.Infrastructure.Interfaces;
using NettoRetail.Shared.Interfaces.Repositories;

namespace NettoRetail.Application.CQRS.Sessions.Commands;

public record UpdateSessionTimeCommand(SessionUpdateModel Model) : IRequest;

public class UpdateSessionTimeCommandHandler : IRequestHandler<UpdateSessionTimeCommand>
{
    private readonly ISessionRepository _sessionRepository;
    private readonly ISessionModelFactory _sessionModelFactory;
    private readonly IIpInfoService _ipInfoService;

    public UpdateSessionTimeCommandHandler(ISessionRepository sessionRepository, ISessionModelFactory sessionModelFactory, IIpInfoService ipInfoService)
    {
        _sessionRepository = sessionRepository;
        _sessionModelFactory = sessionModelFactory;
        _ipInfoService = ipInfoService;
    }

    public async Task Handle(UpdateSessionTimeCommand request, CancellationToken cancellationToken)
    {
        var specification = new GetSessionsByJwtSpecification(request.Model.CurrentJwt);
        await _sessionRepository.UpdateAsync(specification, request.Model.CurrentJwt, request.Model.LastActivityTime, cancellationToken);
    }
}