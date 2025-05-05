namespace NettoRetail.Application.Interfaces;

public interface INotifier
{
    Task NotifyAsync(Guid accountId, string message, CancellationToken cancellationToken);
}
