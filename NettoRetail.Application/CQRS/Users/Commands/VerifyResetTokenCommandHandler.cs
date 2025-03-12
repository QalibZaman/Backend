using MediatR;
using NettoRetail.Application.Exceptions;
using NettoRetail.Application.Interfaces;

namespace NettoRetail.Application.CQRS.Users.Commands;

public record VerifyResetTokenCommand(string Token) : IRequest;
public class VerifyResetTokenCommandHandler(IJwtManager jwtManager)
    : IRequestHandler<VerifyResetTokenCommand>
{
    public Task Handle(VerifyResetTokenCommand request, CancellationToken cancellationToken)
    {
        _ = jwtManager.Validate(request.Token) ??
            throw new InvalidTokenCredentialsException("Invalid or expired reset token.");

        return Task.CompletedTask;
    }
}
