using NettoRetail.Domain.Entities;

namespace NettoRetail.Shared.Interfaces.Repositories;

public interface IPaymentHistoryRepository : IBaseRepository<PaymentHistoryEntity>
{
    Task<List<string>> GetExistingTransactionIdsAsync(List<string> transactionIds, CancellationToken cancellationToken);
    Task AddManyAsync(List<PaymentHistoryEntity> entities, CancellationToken cancellationToken);
}
