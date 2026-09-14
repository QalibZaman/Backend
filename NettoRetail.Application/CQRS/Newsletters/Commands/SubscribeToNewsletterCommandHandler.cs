using MediatR;
using Microsoft.Extensions.Logging;
using NettoRetail.Application.Exceptions;
using NettoRetail.Application.Interfaces;
using NettoRetail.DataAccess.Specifications.Staff;
using NettoRetail.Infrastructure.Interfaces;
using NettoRetail.Shared.Interfaces.Repositories;

namespace NettoRetail.Application.CQRS.Newsletters.Commands;

public record SubscribeToNewsletterCommand(Guid UserId) : IRequest;

public class SubscribeToNewsletterCommandHandler(IStaffRepository staffRepository, IEmailBuilderService emailBuilderService,   IEmailSender emailSender,
    ILogger<SubscribeToNewsletterCommandHandler> logger) : IRequestHandler<SubscribeToNewsletterCommand>
{
    public async Task Handle(SubscribeToNewsletterCommand request, CancellationToken cancellationToken)
    {
        var accountSpecification = new GetStaffByIdSpecification(request.UserId);
        var account = await staffRepository.GetAsync(accountSpecification, cancellationToken);
        if (account is null)
        {
            throw new NotFoundException("Account not found");
        }

        if (account.IsSubscribedToNewsletter)
        {
            throw new BadRequestException("You are already subscribed to newsletters");
        }

        var emailModel = await emailBuilderService.BuildNewsletterSubscribedEmailAsync(account.Email, account.FullName, request.UserId);
        try
        {
            await emailSender.SendAsync(emailModel, request.UserId);
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Failed to send newsletter confirmation email.");
            throw new BadRequestException(
                $"Email send failed: {ex.GetType().FullName} | {ex.Message} | {ex.InnerException?.GetType().FullName} | {ex.InnerException?.Message}");
        }

        account.IsSubscribedToNewsletter = true;
        await staffRepository.UpdateAsync(account, cancellationToken);
    }
}