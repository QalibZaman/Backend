using MediatR;
using Microsoft.Azure.Cosmos;
using NettoRetail.Application.CQRS.Sessions.Commands;
using NettoRetail.Application.CQRS.Users.Commands.Auth;
using NettoRetail.Application.Exceptions;
using NettoRetail.Application.Interfaces;
using NettoRetail.Application.Models.Authentication;
using NettoRetail.Application.Models.Response;
using NettoRetail.Application.Services;
using NettoRetail.DataAccess.Repositories;
using NettoRetail.DataAccess.Specifications.Staff;
using NettoRetail.DataAccess.Specifications.Customer;
using NettoRetail.Domain.Enums;
using NettoRetail.Shared.Interfaces.Repositories;
using NettoRetail.Shared.Utilities;

namespace NettoRetail.Application.CQRS.Customers.Commands;

public record LogInCustomerCommand(LoginModel Model, string IpAddress, string UserAgent, int ShopId) : IRequest<TokenResponse>;

public class LogInCustomerCommandHandler(ICustomerRepository customerRepository,
    IFailedAuthAttemptRepository loginAttemptRepository,
    IFailedAuthAttemptService loginAttemptService,
    IJwtManager jwtManager,
    IMediator mediator) : IRequestHandler<LogInCustomerCommand, TokenResponse>
{
    public async Task<TokenResponse> Handle(LogInCustomerCommand request, CancellationToken cancellationToken)
    {
        var specification = new GetCustomerByEmailAndShopIdSpecification(request.Model.Email, request.ShopId);
        var customer = await customerRepository.GetAsync(specification, cancellationToken)
            ?? throw new NotFoundException("Customer not found");

        var loginSpecification = new GetFailedAuthAttemptByAccountIdSpecification(customer.Id);
        var loginAttempt = await loginAttemptRepository.GetAsync(loginSpecification, cancellationToken);

        if (loginAttemptService.IsAccountLocked(loginAttempt!))
        {
            throw new MyUnauthorizedException("Account is locked. Try again later.");
        }

        if (customer.Status == StaffStatusEnum.Deactivated)
        {
            throw new MyUnauthorizedException("Your account has been deactivated.");
        }

        if (loginAttemptService.HasBlockDurationElapsed(loginAttempt!))
        {
            await loginAttemptService.ResetFailedAttemptsAsync(loginAttempt!, cancellationToken);
        }

        if (!PasswordHasher.VerifyPassword(request.Model.Password, customer.PasswordHash))
        {
            await loginAttemptService.IncrementFailedAttemptsAsync(loginAttempt!, customer.Id, cancellationToken);
            throw new MyUnauthorizedException("Invalid email or password.");
        }

        if (loginAttempt != null)
        {
            await loginAttemptService.ResetFailedAttemptsAsync(loginAttempt, cancellationToken);
        }

        var tokenResponse = jwtManager.GetTokenResponse(customer.Id.ToString(), customer.Email, customer.RoleId.ToString(), customer.ShopId);

        if (customer.Status != StaffStatusEnum.Online)
        {
            customer.Status = StaffStatusEnum.Online;
            await customerRepository.UpdateAsync(customer, cancellationToken);
        }

        var ip = request.IpAddress;

        var sessionCreateCommand = new CreateSessionCommand(
            ip,
            request.UserAgent ?? string.Empty,
            tokenResponse.AccessToken,
            tokenResponse.RefreshToken,
            UserTypeEnum.Customer);

        await mediator.Send(sessionCreateCommand, cancellationToken);

        return tokenResponse;
    }
}
