namespace NettoRetail.Application.Interfaces;
public interface ISessionTerminationService
{
    Task TerminateSessionsAsync(Guid accountId, CancellationToken cancellationToken);
}