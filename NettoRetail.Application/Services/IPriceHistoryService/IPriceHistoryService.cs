public interface IPriceHistoryService
{
    Task<int> GetCurrentPriceAsync(int planId, CancellationToken cancellationToken = default);
    Task<DateTime> CalculateEffectiveDateAsync(DateTime changeDate);
    Task UpdatePlanPriceAsync(int planId, int newPrice, DateTime effectiveDate, CancellationToken cancellationToken = default);
}
