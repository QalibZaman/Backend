using MediatR;
using Microsoft.AspNetCore.Identity;
using NettoRetail.Application.Exceptions;
using NettoRetail.Application.Interfaces;
using NettoRetail.Application.Models.Authentication;
using NettoRetail.Application.Models.Response;
using NettoRetail.DataAccess.Identity;
using NettoRetail.DataAccess.Specifications.Staff;
using NettoRetail.Shared.Constants;
using NettoRetail.Shared.Interfaces.Repositories;

namespace NettoRetail.Application.CQRS.Users.Commands.Auth;

public record LoginCommand(LoginModel Model) : IRequest<TokenResponse>;

public class LoginCommandHandler(UserManager<ApplicationUser> userManager, IFailedAuthAttemptRepository repository,
    IFailedAuthAttemptService loginAttemptService,  IJwtManager jwtManager,
    IStaffRepository staffRepository) : IRequestHandler<LoginCommand, TokenResponse>
{
    public async Task<TokenResponse> Handle(LoginCommand request, CancellationToken cancellationToken)
    {
        var identityUser = await userManager.FindByEmailAsync(request.Model.Email);
        if (identityUser == null)
        {
            throw new MyUnauthorizedException(ValidationErrorMessages.Staff.InvalidLoginErrorMessage);
        }

        var staffSpecification = new GetStaffByIdSpecification(identityUser.Id);
        var user = await staffRepository.GetAsync(staffSpecification, cancellationToken);

        //var specification = new GetFailedAuthAttemptByStaffIdSpecification(identityUser.Id);
        //var loginAttempt = await repository.GetAsync(specification, cancellationToken);

        // if (loginAttemptService.IsAccountLocked(loginAttempt))
        // {
        //     throw new MyUnauthorizedException(ValidationErrorMessages.Staff.AccountLockedErrorMessage);
        // }
        //
        // if (loginAttemptService.HasBlockDurationElapsed(loginAttempt!))
        // {
        //     await loginAttemptService.ResetFailedAttemptsAsync(loginAttempt!, cancellationToken);
        // }

        var result = await userManager.CheckPasswordAsync(identityUser, request.Model.Password);
        if (!result)
        {
            // loginAttempt = await loginAttemptService.IncrementFailedAttemptsAsync(loginAttempt, identityUser.Id, cancellationToken);
            // var availableAttempts = loginAttemptService.AvailableAttempts(loginAttempt);
            // if (availableAttempts == 0)
            // {
            //     throw new MyUnauthorizedException(ValidationErrorMessages.Staff.LockedAccountEmailErrorMessage);
            // }

            throw new MyUnauthorizedException(ValidationErrorMessages.Staff.InvalidLoginErrorMessage);
        }

        // if (loginAttempt != null)
        // {
        //     await loginAttemptService.ResetFailedAttemptsAsync(loginAttempt, cancellationToken);
        // }

        var tokenResponse = jwtManager.GetTokenResponse(identityUser.Id.ToString(), identityUser.Email, user.RoleId.ToString(), user.ShopId);

        return tokenResponse;
    }
}