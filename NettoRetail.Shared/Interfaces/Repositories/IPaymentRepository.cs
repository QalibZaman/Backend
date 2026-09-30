using NettoRetail.Domain.Entities;

namespace NettoRetail.Shared.Interfaces.Repositories;

public interface IPaymentRepository : IBaseRepository<PaymentMethodEntity>
{
    Task<bool> HasAnyByAccountIdAsync(Guid accountId, CancellationToken cancellationToken);

    Task<bool> ExistsDuplicateCardAsync(
        Guid accountId,
        string cardBrand,
        string fourLastCardNumbers,
        long cardExpMonth,
        long cardExpYear,
        CancellationToken cancellationToken);
}