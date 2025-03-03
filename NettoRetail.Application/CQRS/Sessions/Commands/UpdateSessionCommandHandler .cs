using MediatR;
using NettoRetail.Application.Exceptions;
using NettoRetail.Application.Models.Session;
using NettoRetail.Application.Services.ISessionModelFactory;
using NettoRetail.DataAccess.Specifications;
using NettoRetail.Infrastructure.Interfaces;
using NettoRetail.Shared.Interfaces.Repositories;

namespace NettoRetail.Application.CQRS.Sessions.Commands;

public record UpdateSessionCommand(SessionUpdateModel Model) : IRequest;

public class UpdateSessionCommandHandler : IRequestHandler<UpdateSessionCommand>
{
    private readonly ISessionRepository _sessionRepository;
    private readonly ISessionModelFactory _sessionModelFactory;
    private readonly IIpInfoService _ipInfoService;

    public UpdateSessionCommandHandler(ISessionRepository sessionRepository, ISessionModelFactory sessionModelFactory, IIpInfoService ipInfoService)
    {
        _sessionRepository = sessionRepository;
        _sessionModelFactory = sessionModelFactory;
        _ipInfoService = ipInfoService;
    }

    public async Task Handle(UpdateSessionCommand request, CancellationToken cancellationToken)
    {
        if (request.Model.CurrentRefreshToken is null)
            throw new InvalidRefreshTokenException("Failed to update session"); // this is very bad, but again, idk how authorization would work in our case

        var specification = new GetSessionsByRefreshTokenSpecification(request.Model.CurrentRefreshToken);
        await _sessionRepository.UpdateAsync(specification, request.Model.CurrentJwt, request.Model.LastActivityTime, cancellationToken);
    }
}