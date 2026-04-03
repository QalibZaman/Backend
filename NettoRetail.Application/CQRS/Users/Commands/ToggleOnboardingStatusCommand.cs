using MediatR;
using NettoRetail.Application.Exceptions;
using NettoRetail.DataAccess.Specifications.Staff;
using NettoRetail.Shared.Interfaces.Repositories;

namespace NettoRetail.Application.CQRS.Users.Commands;

public record ToggleOnboardingStatusCommand(Guid UserId, bool IsOnboardingCompleted) : IRequest;

public class ToggleOnboardingStatusCommandHandler(IStaffRepository accountRepository) : IRequestHandler<ToggleOnboardingStatusCommand>
{
    public async Task Handle(ToggleOnboardingStatusCommand request, CancellationToken cancellationToken)
    {
        var specification = new GetStaffByIdSpecification(request.UserId);
        var user = await accountRepository.GetAsync(specification, cancellationToken) ?? throw new NotFoundException("User not found.");

        user.IsOnboardingCompleted = request.IsOnboardingCompleted;

        await accountRepository.UpdateAsync(user, cancellationToken);
    }
}
