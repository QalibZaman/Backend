using Mapster;
using MediatR;
using NettoRetail.Application.Models.Entities;
using NettoRetail.Application.Services.ISessionModelFactory;
using NettoRetail.DataAccess.Repositories;
using NettoRetail.Domain.Entities;
using NettoRetail.Infrastructure.Interfaces;
using NettoRetail.Infrastructure.Models;
using NettoRetail.Shared.Interfaces.Repositories;

namespace NettoRetail.Application.CQRS.Sessions.Commands;

public record CreateSessionCommand(string Ips, string UserAgent, string Jwt, string RefreshToken) : IRequest;

public class CreateSessionCommandHandler : IRequestHandler<CreateSessionCommand>
{
    private readonly ISessionRepository _sessionRepository;
    private readonly ISessionModelFactory _sessionModelFactory;
    private readonly IIpInfoService _ipInfoService;

    public CreateSessionCommandHandler(ISessionRepository sessionRepository, ISessionModelFactory sessionModelFactory, IIpInfoService ipInfoService)
    {
        _sessionRepository = sessionRepository;
        _sessionModelFactory = sessionModelFactory;
        _ipInfoService = ipInfoService;
    }

    public async Task Handle(CreateSessionCommand request, CancellationToken cancellationToken)
    {
        var ip = request.Ips.Split(',')[0].Trim();

        Location? location = await _ipInfoService.GetIpInfoAsync(ip);

        var sessionModel = await _sessionModelFactory.CreateSessionModel(request.UserAgent, ip, request.Jwt, request.RefreshToken, location, cancellationToken);

        var session = sessionModel.Adapt<SessionEntity>();

        await _sessionRepository.AddAsync(session, cancellationToken);
    }
}