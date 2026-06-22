using MediatR;
using NettoRetail.Application.Exceptions;
using NettoRetail.Application.Interfaces;
using NettoRetail.Application.Models.Authentication;
using NettoRetail.Application.Models.Response;
using NettoRetail.DataAccess.Specifications.Customer;
using NettoRetail.DataAccess.Specifications.Staff;
using NettoRetail.Domain.Enums;
using NettoRetail.Shared.Constants;
using NettoRetail.Shared.Interfaces.Repositories;
using NettoRetail.Shared.Utilities;
namespace NettoRetail.Application.CQRS.Users.Commands.Auth;

public record LoginCommand(LoginModel Model) : IRequest<TokenResponse>;

public class LoginCommandHandler(IStaffRepository staffRepository,
    IJwtManager jwtManager,
    ICustomerRepository customerRepository,
    IFailedAuthAttemptRepository loginAttemptRepository,
    IFailedAuthAttemptService loginAttemptService)
    : IRequestHandler<LoginCommand, TokenResponse>
{
    public async Task<TokenResponse> Handle(LoginCommand request, CancellationToken cancellationToken)
    {
        var specification = new GetStaffByEmailSpecification(request.Model.Email);
        var user = await staffRepository.GetAsync(specification, cancellationToken)
            ?? throw new BadRequestException("Invalid email or password.");

        var loginSpecification = new GetFailedAuthAttemptByAccountIdSpecification(user.Id);
        var loginAttempt = await loginAttemptRepository.GetAsync(loginSpecification, cancellationToken);

        if (loginAttemptService.IsAccountLocked(loginAttempt!))
        {
            throw new MyUnauthorizedException("Account is locked. Try again later.");
        }

        if (user.Status == StaffStatusEnum.Deactivated)
        {
            throw new MyUnauthorizedException("Your account has been deactivated.");
        }

        if (user.Status == StaffStatusEnum.Pending || string.IsNullOrWhiteSpace(user.PasswordHash))
        {
            throw new MyUnauthorizedException("Your account isn't activated. Please check your email to complete activation.");
        }

        if (loginAttemptService.HasBlockDurationElapsed(loginAttempt!))
        {
            await loginAttemptService.ResetFailedAttemptsAsync(loginAttempt!, cancellationToken);
        }

        if (!PasswordHasher.VerifyPassword(request.Model.Password, user.PasswordHash))
        {
            await loginAttemptService.IncrementFailedAttemptsAsync(loginAttempt!, user.Id, cancellationToken);
            throw new BadRequestException("Invalid email or password.");
        }

        if (loginAttempt != null)
        {
            await loginAttemptService.ResetFailedAttemptsAsync(loginAttempt, cancellationToken);
        }

        if (user.RoleId == RoleConstants.Customer)
        {
            var customer = await customerRepository.GetAsync(
                new GetCustomerByIdSpecification(user.Id),
                cancellationToken);

            if (customer == null)
            {
                throw new MyUnauthorizedException("Customer account not found.");
            }

            if (!customer.IsActive)
            {
                throw new MyUnauthorizedException("Your email isn't verified. Please check your email to complete Sign up.");
            }
        }

        if (user.TwoFactorEnabled)
        {
            var tempToken = jwtManager.GenerateTempToken(user.Id.ToString());
            return new TokenResponse(null, null, tempToken);
        }

        var tokenResponse = jwtManager.GetTokenResponse(user.Id.ToString(), user.Email, user.RoleId.ToString(), user.ShopId);

        return tokenResponse;
    }
}
