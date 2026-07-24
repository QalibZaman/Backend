using Microsoft.Extensions.Options;
using NettoRetail.Application.Interfaces;
using NettoRetail.Domain.Entities;
using NettoRetail.Shared.Interfaces.Repositories;
using NettoRetail.Shared.Options;

namespace NettoRetail.Application.Services;

public class ActivationEmailAttemptAttemptService(IActivationEmailAttemptRepository emailAttemptRepository, IOptions<EmailSendBlockOptions> emailOptions) : IActivationEmailAttemptService
{
    private readonly EmailSendBlockOptions _emailOptions = emailOptions.Value;
    public async Task IncrementSentEmailsAsync(ActivationEmailAttemptEntity?activationEmailEntity, Guid staffId, CancellationToken cancellationToken)
    {
        if (activationEmailEntity == null)
        {
            activationEmailEntity = new ActivationEmailAttemptEntity
            {
                StaffId = staffId,
                SendCount = 1,
                LastSentAt = DateTime.UtcNow
            };
            await emailAttemptRepository.AddAsync(activationEmailEntity, cancellationToken);
        }
        else
        {
            activationEmailEntity.SendCount++;
            activationEmailEntity.LastSentAt = DateTime.UtcNow;
            await emailAttemptRepository.UpdateAsync(activationEmailEntity, cancellationToken);
        }
    }

    public bool HasBlockDurationElapsed(ActivationEmailAttemptEntity? activationEmailEntity)
    {
        return activationEmailEntity != null &&
               activationEmailEntity.LastSentAt.AddHours(_emailOptions.BlockDurationInHours) < DateTime.UtcNow;
    }

    public bool HasAttemptsRemaining(ActivationEmailAttemptEntity? activationEmailEntity)
    {
        return activationEmailEntity != null &&
               activationEmailEntity.SendCount >= _emailOptions.SentEmailsThreshold;
    }

    public async Task ResetAttemptsAsync(ActivationEmailAttemptEntity activationEmailEntity, CancellationToken cancellationToken)
    {
        activationEmailEntity.SendCount = 0;
        activationEmailEntity.LastSentAt = default;

        await emailAttemptRepository.UpdateAsync(activationEmailEntity, cancellationToken);
    }
}