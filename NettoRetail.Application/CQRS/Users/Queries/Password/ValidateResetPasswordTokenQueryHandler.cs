using System.Security.Claims;
using MediatR;
using Microsoft.AspNetCore.Identity;
using NettoRetail.Application.Exceptions;
using NettoRetail.Application.Interfaces;
using NettoRetail.DataAccess.Identity;
using NettoRetail.DataAccess.Specifications.Staff;
using NettoRetail.Shared.Interfaces.Repositories;

namespace NettoRetail.Application.CQRS.Users.Queries.Password;

public record ValidateResetPasswordTokenQuery(string Token) : IRequest;

public class ValidateResetPasswordTokenQueryHandler(IJwtManager jwtManager, UserManager<ApplicationUser> userManager, IStaffRepository accountRepository)
    : IRequestHandler<ValidateResetPasswordTokenQuery>
{
    public async Task Handle(ValidateResetPasswordTokenQuery request, CancellationToken cancellationToken)
    {
        var claimsPrincipal = jwtManager.ValidateResetToken(request.Token);

        var email = claimsPrincipal.FindFirst(ClaimTypes.Email)?.Value
                    ?? throw new BadRequestException("Invalid token payload.");

        var identityUser = await userManager.FindByEmailAsync(email)
                           ?? throw new NotFoundException("User not found.");

        var specification = new GetStaffByEmailSpecification(email);

        var user = await accountRepository.GetAsync(
                       specification,
                       cancellationToken)
                   ?? throw new NotFoundException("User not found.");
    }
}