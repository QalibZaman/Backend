using System.Security.Claims;
using MediatR;
using NettoRetail.Application.Exceptions;
using NettoRetail.Application.Interfaces;
using NettoRetail.DataAccess.Specifications.Staff;
using NettoRetail.Shared.Interfaces.Repositories;

namespace NettoRetail.Application.CQRS.Users.Commands.Admin;


public record ActivateLockedAccountCommand(string Token) : IRequest;
public class ActivateLockedAccountCommandHandler(IJwtManager jwtManager, IStaffRepository staffRepository, IFailedAuthAttemptRepository failedAuthAttemptRepository, IFailedAuthAttemptService failedAuthAttemptService) : IRequestHandler<ActivateLockedAccountCommand>
{
    public async Task Handle(ActivateLockedAccountCommand request, CancellationToken cancellationToken)
    {
        var principial = jwtManager.ValidateActivateLockedAccountToken(request.Token);
        var email = principial.FindFirst(ClaimTypes.Email)!.Value;
        var specification = new GetStaffByEmailSpecification(email);
        var user = await staffRepository.GetAsync(specification, cancellationToken);
        if (user == null)
        {
            throw new NotFoundException("Account not found.");
        }
        var attemptSpecification = new GetFailedAuthAttemptByStaffIdSpecification(user.Id);
        var attempt = await failedAuthAttemptRepository.GetAsync(attemptSpecification, cancellationToken);

        if (attempt is not null)
        {
           await failedAuthAttemptService.ResetFailedAttemptsAsync(attempt, cancellationToken);
        }
    }
}