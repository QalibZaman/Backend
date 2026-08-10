using MediatR;

using NettoRetail.Application.Exceptions;
using NettoRetail.Application.Interfaces;
using NettoRetail.Application.Models.Email;
using NettoRetail.DataAccess.Specifications.Email;
using NettoRetail.DataAccess.Specifications.Staff;
using NettoRetail.Infrastructure.Interfaces;
using NettoRetail.Shared.Interfaces.Repositories;

namespace NettoRetail.Application.CQRS.Users.Commands.Auth;


public record ResendActivationEmailCommand(ActivationEmailAttemptRequest Model) : IRequest;
public class ResendActivationEmailCommandHandler(ILinkService linkService,
    IEmailBuilderService emailBuilderService,
    IEmailSender emailSender,
    IStaffRepository staffRepository, IActivationEmailAttemptService emailAttemptService, IActivationEmailAttemptRepository emailAttemptRepository) : IRequestHandler<ResendActivationEmailCommand>
{
    public async Task Handle(ResendActivationEmailCommand request, CancellationToken cancellationToken)
    {
        var staffSpecification = new GetStaffByEmailSpecification(request.Model.Email);
        var user = await staffRepository.GetAsync(staffSpecification, cancellationToken);

        if (user == null) return;
        var emailSpecification = new GetActivationEmailAttemptByStaffIdSpecification(user.Id);
        var emailEntity = await emailAttemptRepository.GetAsync(emailSpecification, cancellationToken);

        if (emailEntity != null && emailAttemptService.HasBlockDurationElapsed(emailEntity))
        {
            await emailAttemptService.ResetAttemptsAsync(emailEntity, cancellationToken);
        }

        var isAttemptLeft = emailAttemptService.HasAttemptsRemaining(emailEntity);
        if (!isAttemptLeft)
        {
            throw new BadRequestException("No attempts left, please try again later");
        }

        var link = linkService.CreateBlockedAccountReactivationLink(request.Model.Email, user.Id);
        var emailModel = await emailBuilderService.BuildActivateLockedAccountEmailAsync(request.Model.Email, user.FirstName, link);
        await emailSender.SendAsync(emailModel, user.Id);

        await emailAttemptService.IncrementSentEmailsAsync(emailEntity, user.Id, cancellationToken);
    }
}