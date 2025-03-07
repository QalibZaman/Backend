using System.Security.Claims;
using MediatR;
using NettoRetail.Application.Exceptions;
using NettoRetail.Application.Interfaces;
using NettoRetail.DataAccess.Specifications;
using NettoRetail.Infrastructure.Interfaces;
using NettoRetail.Infrastructure.Models;
using NettoRetail.Shared.Interfaces.Repositories;

namespace NettoRetail.Application.CQRS.Users.Commands;

public record ForgotPasswordCommand(string Email) : IRequest;

public class ForgotPasswordCommandHandler(
    IStaffRepository staffRepository,
    IJwtManager jwtManager,
    IEmailSender emailSender) : IRequestHandler<ForgotPasswordCommand>
{
    public async Task Handle(ForgotPasswordCommand request, CancellationToken cancellationToken)
    {
        var specification = new GetStaffByEmailSpecification(request.Email);

        var user = await staffRepository
            .GetAsync(specification, cancellationToken) ??
            throw new NotFoundException("User with this email was not found.");

        var claims = new List<Claim> { new(ClaimTypes.Email, user.Email) };
        var resetToken = jwtManager.Generate(claims, TimeSpan.FromHours(1));
        var resetLink = $"https://retailtech.com/reset-password?token={resetToken}"; // temporary link

        var emailModel = new EmailModel
        {
            Email = user.Email,
            Subject = "Reset Your Password",
            Message = $"""
            Hi {user.FirstName},

            Click the link below to reset your password:

            {resetLink}

            This link is valid for 1 hour. If you did not request this, please ignore this email.
            """
        };

        await emailSender.SendAsync(emailModel);
    }
}