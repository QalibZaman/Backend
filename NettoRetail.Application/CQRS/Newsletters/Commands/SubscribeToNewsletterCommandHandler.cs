using MediatR;
using Microsoft.Extensions.Logging;
using NettoRetail.Application.Exceptions;
using NettoRetail.Application.Interfaces;
using NettoRetail.Application.Services.ILinkService;
using NettoRetail.DataAccess.Specifications.Account;
using NettoRetail.Domain.Entities;
using NettoRetail.Infrastructure.Interfaces;
using NettoRetail.Infrastructure.Services;
using NettoRetail.Shared.Interfaces.Repositories;

namespace NettoRetail.Application.CQRS.Newsletters.Commands;

public record SubscribeToNewsletterCommand(Guid AccountId) : IRequest<Unit>;

public class SubscribeToNewsletterCommandHandler(IStaffRepository accountRepository,
    IEmailSender emailSender,
    IEmailBuilderService emailBuilderService,
    ILogger<SubscribeToNewsletterCommandHandler> logger) : IRequestHandler<SubscribeToNewsletterCommand,Unit>
{
    public async Task<Unit> Handle(SubscribeToNewsletterCommand request, CancellationToken cancellationToken)
    {
        var account = await accountRepository.GetAsync(new GetStaffByIdSpecification(request.AccountId), cancellationToken);
        ValidateCanSubscribe(account);
        account.IsSubscribedToNewsletter = true;
        await accountRepository.UpdateAsync(account, cancellationToken);

        var emailModel = await emailBuilderService.BuildNewsletterSubscribedEmailAsync(account.Email, account.FullName,account.Id);
        if (string.IsNullOrWhiteSpace(emailModel.Email))
            throw new BadRequestException("Generated EmailModel.Email is empty");

        try
        {
            await emailSender.SendAsync(emailModel, account.Id);
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Failed to send newsletter confirmation email.");
            throw new BadRequestException(
                $"Email send failed: {ex.GetType().FullName} | {ex.Message} | {ex.InnerException?.GetType().FullName} | {ex.InnerException?.Message}");

        }

        return Unit.Value;
    }

    private static void ValidateCanSubscribe(StaffEntity account)
    {
        if (account is null)
            throw new NotFoundException("There is no account with such Id");
        if (account.IsSubscribedToNewsletter)
            throw new BadRequestException("You are already subscribed");

        if (string.IsNullOrWhiteSpace(account.Email))
            throw new BadRequestException("User email is unavailable.");
    }
}