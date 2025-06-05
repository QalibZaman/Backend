using Microsoft.EntityFrameworkCore;
using NettoRetail.DataAccess.Context;
using NettoRetail.Domain.Entities;
using NettoRetail.Shared.Interfaces.Repositories;

namespace NettoRetail.DataAccess.Repositories;
public class PaymentHistoryRepository : BaseRepository<PaymentHistoryEntity>, IPaymentHistoryRepository
{
    public PaymentHistoryRepository(AppDbContext context) : base(context) { }
    public async Task<List<string>> GetExistingTransactionIdsAsync(List<string> transactionIds, CancellationToken cancellationToken)
    {
        return await _dbContext.Set<PaymentHistoryEntity>()
            .Where(p => transactionIds.Contains(p.TransactionId))
            .Select(p => p.TransactionId)
            .ToListAsync(cancellationToken);
    }

    public async Task AddManyAsync(List<PaymentHistoryEntity> entities, CancellationToken cancellationToken)
    {
        await _dbContext.Set<PaymentHistoryEntity>().AddRangeAsync(entities, cancellationToken);
    }

}