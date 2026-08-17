using MediatR;
using Microsoft.Extensions.Logging;
using NettoRetail.Application.Exceptions;
using NettoRetail.Application.Interfaces;
using NettoRetail.DataAccess.Specifications.Staff;
using NettoRetail.Infrastructure.Interfaces;
using NettoRetail.Shared.Interfaces.Repositories;

namespace NettoRetail.Application.CQRS.Newsletters.Commands;

public record UnsubscribeToNewsletterCommand(Guid UserId) : IRequest;

public class UnsubscribeToNewsletterCommandHandler(IStaffRepository staffRepository, IEmailSender emailSender,
    IEmailBuilderService emailBuilderService,
    ILogger<SubscribeToNewsletterCommandHandler> logger) : IRequestHandler<UnsubscribeToNewsletterCommand>
{
    public async Task Handle(UnsubscribeToNewsletterCommand request, CancellationToken cancellationToken)
    {
        var userSpecification = new GetStaffByIdSpecification(request.UserId);
        var user = await staffRepository.GetAsync(userSpecification, cancellationToken);
        if (user == null)
        {
            throw new NotFoundException("Account not found");
        }

        if (!user.IsSubscribedToNewsletter)
        {
            throw new BadRequestException("You are not subscribed to newsletter");
        }

        var currentYear = DateTime.UtcNow.Year;
        var model = await emailBuilderService.BuildNewsletterUnsubscribedEmailAsync(user.Email, user.FullName, currentYear);
        try
        {
            await emailSender.SendAsync(model, request.UserId);
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Failed to send newsletter unsubscription email.");
            throw new BadRequestException(
                $"Email send failed: {ex.GetType().FullName} | {ex.Message} | {ex.InnerException?.GetType().FullName} | {ex.InnerException?.Message}");
        }

        user.IsSubscribedToNewsletter = false;
        await staffRepository.UpdateAsync(user, cancellationToken);
    }
}