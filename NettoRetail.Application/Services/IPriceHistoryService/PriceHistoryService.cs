using NettoRetail.Shared.Interfaces.Repositories;
using NettoRetail.Shared.Specifications;

public class PriceHistoryService(IPriceHistoryRepository priceHistoryRepository) : IPriceHistoryService
{
    public async Task<int> GetCurrentPriceAsync(int planId, CancellationToken cancellationToken)
    {
        var activePrice = await priceHistoryRepository.GetActivePriceAsync(planId, null, cancellationToken);
        return activePrice?.Price ?? 0;
    }

    public Task<DateTime> CalculateEffectiveDateAsync(DateTime changeDate)
    {
        var day = changeDate.Day;
        var nextMonth = new DateTime(changeDate.Year, changeDate.Month, 1).AddMonths(1);

        DateTime effectiveDate;
        if (day >= 1 && day <= 15)
            effectiveDate = nextMonth;
        else
            effectiveDate = nextMonth.AddMonths(1);

        effectiveDate = DateTime.SpecifyKind(effectiveDate, DateTimeKind.Utc);

        return Task.FromResult(effectiveDate);
    }

    public async Task UpdatePlanPriceAsync(int planId, int newPrice, DateTime effectiveDate, CancellationToken cancellationToken)
    {
        effectiveDate = DateTime.SpecifyKind(effectiveDate, DateTimeKind.Utc);

        var activePrice = await priceHistoryRepository.GetAsync(
           new SpecificationBuilder<PriceHistoryEntity>()
               .AddCriteria(ph => ph.SubscriptionPlanId == planId)
               .AddCriteria(ph => ph.EndDateTime == null)
               .Build(),
           cancellationToken);

        if (activePrice != null)
        {
            activePrice.EndDateTime = effectiveDate;
            await priceHistoryRepository.UpdateAsync(activePrice, cancellationToken);
        }

        var newPriceHistory = new PriceHistoryEntity
        {
            SubscriptionPlanId = planId,
            Price = newPrice,
            StartDateTime = effectiveDate,
            EndDateTime = null
        };
        await priceHistoryRepository.AddAsync(newPriceHistory, cancellationToken);
    }
}
