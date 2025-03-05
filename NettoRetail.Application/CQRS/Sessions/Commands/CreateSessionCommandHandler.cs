using Mapster;
using MediatR;
using NettoRetail.Application.Services.ISessionModelFactory;
using NettoRetail.Domain.Entities;
using NettoRetail.Infrastructure.Interfaces;
using NettoRetail.Infrastructure.Models;
using NettoRetail.Shared.Interfaces.Repositories;

namespace NettoRetail.Application.CQRS.Sessions.Commands;

public record CreateSessionCommand(string Ips, string UserAgent, string Jwt, string RefreshToken) : IRequest;

public class CreateSessionCommandHandler(
    ISessionRepository sessionRepository,
    ISessionModelFactory sessionModelFactory,
    IIpInfoService ipInfoService) : IRequestHandler<CreateSessionCommand>
{
    public async Task Handle(CreateSessionCommand request, CancellationToken cancellationToken)
    {
        var ip = request.Ips.Split(',')[0].Trim();

        Location? location;

        if (ip == string.Empty)
            location = null;
        else
            location = await ipInfoService.GetIpInfoAsync(ip);

        var sessionModel = await sessionModelFactory.CreateSessionModel(request.UserAgent, ip, request.Jwt, request.RefreshToken, location, cancellationToken);

        var session = sessionModel.Adapt<SessionEntity>();

        await sessionRepository.AddAsync(session, cancellationToken);
    }
}