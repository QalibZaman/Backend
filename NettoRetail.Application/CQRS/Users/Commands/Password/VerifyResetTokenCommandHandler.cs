using MediatR;
using Microsoft.IdentityModel.Tokens;
using NettoRetail.Application.Exceptions;
using NettoRetail.Application.Interfaces;

namespace NettoRetail.Application.CQRS.Users.Commands.Password;

public record VerifyResetTokenCommand(string Token) : IRequest;

public class VerifyResetTokenCommandHandler(IJwtManager jwtManager)
    : IRequestHandler<VerifyResetTokenCommand>
{
    public Task Handle(VerifyResetTokenCommand request, CancellationToken cancellationToken)
    {
        if (string.IsNullOrWhiteSpace(request.Token))
            throw new BadRequestException("Reset token is required.");

        try
        {
            _ = jwtManager.ValidateResetToken(request.Token);
        }
        catch (SecurityTokenExpiredException)
        {
            throw new MyUnauthorizedException("The recovery link has expired.");
        }
        catch (ArgumentException)
        {
            throw new BadRequestException("Malformed or invalid token.");
        }

        return Task.CompletedTask;
    }
}
