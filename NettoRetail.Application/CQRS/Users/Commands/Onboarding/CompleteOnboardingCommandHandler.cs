using System.Security.Claims;
using MediatR;
using Microsoft.AspNetCore.Http;
using NettoRetail.Application.Exceptions;
using NettoRetail.DataAccess.Specifications.Staff;
using NettoRetail.Shared.Interfaces.Repositories;

namespace NettoRetail.Application.CQRS.Users.Commands.Onboarding
{
    public class CompleteOnboardingCommandHandler(
    IStaffRepository staffRepository,
    IHttpContextAccessor httpContextAccessor)
    : IRequestHandler<CompleteOnboardingCommand>
    {
        public async Task Handle(
            CompleteOnboardingCommand request,
            CancellationToken cancellationToken)
        {
            var userIdClaim = httpContextAccessor.HttpContext?
                .User
                .FindFirst(ClaimTypes.NameIdentifier)?
                .Value;

            if (!Guid.TryParse(userIdClaim, out var userId))
            {
                throw new MyUnauthorizedException("User is not authorized.");
            }

            var specification = new GetStaffByIdSpecification(userId);

            var staff = await staffRepository.GetAsync(specification, cancellationToken);

            if (staff is null)
            {
                throw new NotFoundException("Staff account was not found.");
            }

            staff.IsOnboardingCompleted = true;

            await staffRepository.UpdateAsync(staff, cancellationToken);
        }
    }
}
