using NettoRetail.Domain.Entities;

public interface ISubscriptionNotificationService
{
    Task NotifyPriceChangedAsync(SubscriptionPlanEntity plan, int oldPrice, int newPrice, DateTime effectiveDate, CancellationToken cancellationToken = default);
}
