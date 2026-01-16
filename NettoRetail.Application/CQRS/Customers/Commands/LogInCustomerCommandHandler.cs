using MediatR;
using NettoRetail.Application.CQRS.Sessions.Commands;
using NettoRetail.Application.CQRS.Users.Commands.Auth;
using NettoRetail.Application.Models.Authentication;
using NettoRetail.Application.Models.Response;

namespace NettoRetail.Application.CQRS.Customers.Commands;

public record LogInCustomerCommand(LoginModel Model, string IpAddress, string UserAgent) : IRequest<TokenResponse>;

public class LogInCustomerCommandHandler(IMediator mediator) : IRequestHandler<LogInCustomerCommand, TokenResponse>
{
    public async Task<TokenResponse> Handle(LogInCustomerCommand request, CancellationToken cancellationToken)
    {
        var data = await mediator.Send(new LoginCommand(request.Model), cancellationToken);

        if (!string.IsNullOrEmpty(data.TempToken))
            return data;

        var ip = (request.IpAddress ?? string.Empty).Split(',')[0].Trim();

        var sessionCreateCommand = new CreateSessionCommand(
            ip,
            request.UserAgent ?? string.Empty,
            data.AccessToken,
            data.RefreshToken);

        await mediator.Send(sessionCreateCommand, cancellationToken);

        return data;
    }
}
