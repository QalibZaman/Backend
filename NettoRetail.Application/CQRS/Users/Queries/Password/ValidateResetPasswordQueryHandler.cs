using System.Security.Claims;
using MediatR;
using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.AspNetCore.Identity;
using Microsoft.IdentityModel.Tokens;
using NettoRetail.Application.CQRS.Users.Commands.Password;
using NettoRetail.Application.Exceptions;
using NettoRetail.Application.Interfaces;
using NettoRetail.Application.Models.Request.Password;
using NettoRetail.Application.Models.Response;
using NettoRetail.DataAccess.Identity;
using NettoRetail.DataAccess.Specifications.Staff;
using NettoRetail.Shared.Interfaces.Repositories;
using NettoRetail.Shared.Interfaces.Utilities;

namespace NettoRetail.Application.CQRS.Users.Queries.Password;

public record ValidateResetPasswordQuery(ResetPasswordRequest Model) : IRequest<ValidateResetPasswordResponse>;

public class ValidateResetPasswordQueryHandler(
    IStaffRepository accountRepository,
    IJwtManager jwtManager,
    UserManager<ApplicationUser> userManager)
    : IRequestHandler<ValidateResetPasswordQuery, ValidateResetPasswordResponse>
{
    public async Task<ValidateResetPasswordResponse> Handle(ValidateResetPasswordQuery request, CancellationToken cancellationToken)
    {
        if (request.Model.NewPassword.IsNullOrEmpty()) throw new BadRequestException("New password is required.");
        var claimsPrincipal = jwtManager.ValidateResetToken(request.Model.Token);

        var email = claimsPrincipal.FindFirst(ClaimTypes.Email)?.Value
                    ?? throw new BadRequestException("Invalid token payload.");

        var identityUser = await userManager.FindByEmailAsync(email)
                           ?? throw new NotFoundException("User not found.");

        var specification = new GetStaffByEmailSpecification(email);

        var user = await accountRepository.GetAsync(
                       specification,
                       cancellationToken)
                   ?? throw new NotFoundException("User not found.");

        var passwordHasher = userManager.PasswordHasher;
        // Check if new password is the same as current password
        var currentPasswordResult = passwordHasher.VerifyHashedPassword(
            identityUser,
            identityUser.PasswordHash!,
            request.Model.NewPassword);

        var result = currentPasswordResult != PasswordVerificationResult.Success;
        return new ValidateResetPasswordResponse
        {
            MatchesCurrentPassword = !result,
        };
    }
}