namespace NettoRetail.Shared.Interfaces.Repositories
{
    public interface IPriceHistoryRepository : IBaseRepository<PriceHistoryEntity>
    {
        Task<PriceHistoryEntity?> GetActivePriceAsync(int planId, DateTime? date = null, CancellationToken cancellationToken = default);
    }
}
