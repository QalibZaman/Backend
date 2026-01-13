using System.ComponentModel.DataAnnotations;
using MediatR;
using NettoRetail.Application.Exceptions;
using NettoRetail.Application.Interfaces;
using NettoRetail.DataAccess.Specifications.InvitationToken;
using NettoRetail.Domain.Entities;
using NettoRetail.Shared.Interfaces.Repositories;

namespace NettoRetail.Application.CQRS.InvitationToken;

public record ValidateInvitationTokenCommand(string Token) : IRequest;

public class ValidateInvitationTokenHandler(
    IInvitationTokenRepository invitationTokenRepository,
    IInvitationTokenService invitationTokenService) : IRequestHandler<ValidateInvitationTokenCommand>
{
    public async Task Handle(ValidateInvitationTokenCommand request, CancellationToken cancellationToken)
    {
        if (string.IsNullOrWhiteSpace(request.Token))
        {
            throw new ValidationException("Invitation token is empty");
        }

        var now = DateTime.UtcNow;
        var hash = invitationTokenService.Hash(request.Token);

        var token = await invitationTokenRepository.GetByHashAsync(
            new GetInvitationTokenByHashSpecification(hash),
            cancellationToken) ?? throw new NotFoundException("Invitation token not found");

        Validate(token, now);
    }

    private static void Validate(InvitationTokenEntity token, DateTime now)
    {
        if (token.UsedAt is not null || token.InvalidatedAt is not null || token.ExpiresAt <= now)
        {
            throw new ConflictException("Your invitation link is invalid or expired.");
        }
    }
}