using MediatR;
using Microsoft.AspNetCore.Identity;
using NettoRetail.Application.Exceptions;
using NettoRetail.Application.Interfaces;
using NettoRetail.DataAccess.Identity;
using NettoRetail.DataAccess.Specifications.Staff;
using NettoRetail.Infrastructure.Interfaces;
using NettoRetail.Shared.Constants;
using NettoRetail.Shared.Interfaces.Repositories;
using NettoRetail.Shared.Interfaces.Utilities;

namespace NettoRetail.Application.CQRS.Users.Commands.Staff;

public record DeleteStaffCommand(Guid Id, string Password) : IRequest;

public class DeleteStaffCommandHandler(
    IStaffRepository staffRepository,
    ISessionTerminationService sessionTerminationService,
    IUnitOfWork unitOfWork,
    UserManager<ApplicationUser> userManager,
    IFailedAuthAttemptRepository failedAuthAttemptRepository,
    IFailedAuthAttemptService failedAuthAttemptService,
    IEmailBuilderService emailBuilderService,
    IEmailSender emailSender)
    : IRequestHandler<DeleteStaffCommand>
{
    public async Task Handle(DeleteStaffCommand request, CancellationToken cancellationToken)
    {
        if (string.IsNullOrWhiteSpace(request.Password))
        {
            throw new BadRequestException(StaffConstants.IncorrectPassword);
        }

        var specification = new GetStaffByIdSpecification(request.Id);
        var account = await staffRepository.GetAsync(specification, cancellationToken)
            ?? throw new NotFoundException(StaffConstants.StaffMemberNotFound);

        var identityUser = await userManager.FindByIdAsync(account.Id.ToString())
            ?? throw new NotFoundException(StaffConstants.StaffMemberNotFound);

        var loginAttemptSpec = new GetFailedAuthAttemptByStaffIdSpecification(account.Id);
        var failedAttempt = await failedAuthAttemptRepository.GetAsync(loginAttemptSpec, cancellationToken);

        if (failedAttempt != null && failedAuthAttemptService.IsAccountLocked(failedAttempt))
        {
            throw new MyUnauthorizedException(StaffConstants.AccountLocked, failedAttempt.LockoutEndTime);
        }

        var isPasswordValid = await userManager.CheckPasswordAsync(identityUser, request.Password);
        if (!isPasswordValid)
        {
            var updatedAttempt = await failedAuthAttemptService.IncrementFailedAttemptsAsync(failedAttempt, account.Id, cancellationToken);

            if (failedAuthAttemptService.IsAccountLocked(updatedAttempt))
            {
                var lockoutEmail = await emailBuilderService.BuildFailedDeletionAttemptsEmailAsync(account.Email, account.FirstName);
                await emailSender.SendAsync(lockoutEmail, account.Id);

                throw new MyUnauthorizedException(StaffConstants.AccountLocked, updatedAttempt.LockoutEndTime);
            }

            throw new BadRequestException(StaffConstants.IncorrectPassword);
        }

        if (failedAttempt != null)
        {
            await failedAuthAttemptService.ResetFailedAttemptsAsync(failedAttempt, cancellationToken);
        }

        using var transaction = unitOfWork.BeginTransaction();

        await sessionTerminationService.TerminateSessionsAsync(account.Id, cancellationToken);
        await staffRepository.DeleteAsync(account, cancellationToken);

        var identityDeleteResult = await userManager.DeleteAsync(identityUser);
        if (!identityDeleteResult.Succeeded)
        {
            throw new BadRequestException(string.Join(", ", identityDeleteResult.Errors.Select(e => e.Description)));
        }

        transaction.Commit();

        var deletedEmail = await emailBuilderService.BuildAccountDeletedEmailAsync(account.Email, account.FirstName);
        await emailSender.SendAsync(deletedEmail, account.Id);
    }
}