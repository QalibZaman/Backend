using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using MediatR;
using Microsoft.Extensions.Options;
using NettoRetail.Application.Exceptions;
using NettoRetail.Application.Interfaces;
using NettoRetail.DataAccess.Specifications.Account;
using NettoRetail.Infrastructure.Interfaces;
using NettoRetail.Infrastructure.Options;
using NettoRetail.Shared.Constants;
using NettoRetail.Shared.Interfaces.Repositories;

namespace NettoRetail.Application.CQRS.Users.Commands.Password;

public record SendPasswordResetLinkCommand(string Email) : IRequest;

public class SendPasswordResetLinkCommandHandler(
    IAccountRepository accountRepo,
    IJwtManager jwtManager,
    IEmailBuilderService emailBuilder,
    IEmailSender emailSender,
    IOptions<ResetPasswordOptions> resetOpts
) : IRequestHandler<SendPasswordResetLinkCommand>
{
    private readonly ResetPasswordOptions _opts = resetOpts.Value;

    public async Task Handle(
        SendPasswordResetLinkCommand request,
        CancellationToken cancellationToken)
    {
        var email = request.Email.Trim().ToLowerInvariant();
        var account = await accountRepo.GetAsync(
            new GetAccountByEmailSpecification(email),
            cancellationToken);

        if (account is null)
            throw new BadRequestException("No account with that email.");

        var claims = new List<Claim>
        {
            new Claim(ClaimTypes.NameIdentifier, account.Id.ToString()),
            new Claim(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString()),
            new Claim("type", AuthTokenTypes.Reset)
        };

        var resetToken = jwtManager.Generate(
            claims,
            TimeSpan.FromHours(_opts.ActivationTokenExpirationHours)
        );

        var link = string.Format(
            _opts.ResetLink,
            Uri.EscapeDataString(resetToken)
        );

        var emailModel = await emailBuilder
            .BuildPlatformAdminActivationEmailAsync(email, account.FirstName, link);
        await emailSender.SendAsync(emailModel, account.Id);
    }

}
