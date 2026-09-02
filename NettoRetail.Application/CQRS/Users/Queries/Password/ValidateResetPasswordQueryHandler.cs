using System.Security.Claims;
using MediatR;
using Microsoft.AspNetCore.Identity;
using NettoRetail.Application.Exceptions;
using NettoRetail.Application.Interfaces;
using NettoRetail.DataAccess.Identity;
using NettoRetail.DataAccess.Specifications.Staff;
using NettoRetail.Shared.Interfaces.Repositories;

namespace NettoRetail.Application.CQRS.Users.Commands.Password;

public record ValidateResetPasswordQuery(string Token) : IRequest;

public class ValidateResetPasswordQueryHandler(IJwtManager jwtManager, UserManager<ApplicationUser> userManager, IStaffRepository accountRepository)
    : IRequestHandler<ValidateResetPasswordQuery>
{
    public async Task Handle(ValidateResetPasswordQuery request, CancellationToken cancellationToken)
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