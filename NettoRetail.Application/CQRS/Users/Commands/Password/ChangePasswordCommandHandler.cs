using MediatR;
using Microsoft.AspNetCore.Identity;
using NettoRetail.Application.Exceptions;
using NettoRetail.Application.Interfaces;
using NettoRetail.Application.Models.Request.Password;
using NettoRetail.DataAccess.Identity;
using NettoRetail.DataAccess.Specifications.Staff;
using NettoRetail.Infrastructure.Interfaces;
using NettoRetail.Shared.Interfaces.Repositories;

namespace NettoRetail.Application.CQRS.Users.Commands.Password;

public record ChangePasswordCommand(Guid UserId, ChangePasswordRequest Model) : IRequest;

public class ChangePasswordCommandHandler(
    UserManager<ApplicationUser> userManager,
    IEmailBuilderService emailBuilderService,
    IEmailSender emailSender,
    IStaffRepository staffRepository)
    : IRequestHandler<ChangePasswordCommand>
{
    public async Task Handle(ChangePasswordCommand request, CancellationToken cancellationToken)
    {
        var identityUser = await userManager.FindByIdAsync(request.UserId.ToString());
        if (identityUser is null)
        {
            throw new NotFoundException("User not found.");
        }

        var specification = new GetStaffByIdSpecification(request.UserId);
        var user = await staffRepository.GetAsync(specification, cancellationToken);

        var result =
            await userManager.ChangePasswordAsync(identityUser, request.Model.CurrentPassword,
                request.Model.NewPassword);
        if (!result.Succeeded)
        {
            throw new BadRequestException(string.Join(", ", result.Errors.Select(e => e.Description)));
        }

        var emailModel = await emailBuilderService.BuildChangePasswordEmailAsync(user.Email, user.FirstName);
        try
        {
            await emailSender.SendAsync(emailModel, request.UserId);
        }
        catch (Exception ex)
        {
            throw new BadRequestException(
                $"Email send failed: {ex.GetType().FullName} | {ex.Message} | {ex.InnerException?.GetType().FullName} | {ex.InnerException?.Message}");
        }
    }
}