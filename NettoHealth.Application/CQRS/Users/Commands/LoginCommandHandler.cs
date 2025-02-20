using System.Security.Claims;
using MediatR;
using NettoHealth.Application.Exceptions;
using NettoHealth.Application.Models.Entities;
using NettoHealth.Application.Models.Response;
using NettoHealth.DataAccess.Specifications;
using NettoHealth.Shared.Interfaces.Repositories;
using NettoHealth.Shared.Interfaces.Utilities;
using NettoHealth.Shared.Utilities;

namespace NettoHealth.Application.CQRS.Users.Commands;

public record LoginCommand(LoginModel Model) : IRequest<ServiceResponse<TokenResponse>>;

public class LoginCommandHandler(IHealthcareProviderRepository repository, IJwtManager jwtManager)
    : IRequestHandler<LoginCommand, ServiceResponse<TokenResponse>>
{
    public async Task<ServiceResponse<TokenResponse>> Handle(LoginCommand request, CancellationToken cancellationToken)
    {
        var specification = new GetHealthcareProviderByEmailSpecification(request.Model.Email);
        var user = await repository.GetAsync(specification, cancellationToken);

        if (user == null) throw new NotFoundException("User was not found");

        if (!PasswordHasher.VerifyPassword(request.Model.Password, user.PasswordHash))
        {
            throw new MyUnauthorizedException("Password is incorrect");
        }

        var claims = new List<Claim>
        {
            new(ClaimTypes.NameIdentifier, user.Id.ToString()),
            new(ClaimTypes.Email, user.Email)
        };

        var accessToken = jwtManager.Generate(claims, TimeSpan.FromMinutes(30));
        var refreshToken = jwtManager.Generate(claims, TimeSpan.FromDays(7));

        var tokenResponse = new TokenResponse(accessToken, refreshToken);

        return new ServiceResponse<TokenResponse>("Login successful", tokenResponse, true);
    }
}