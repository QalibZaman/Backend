using NettoRetail.Domain.Entities;

namespace NettoRetail.Shared.Interfaces.Repositories;

public interface IPaymentRepository : IBaseRepository<PaymentMethodEntity>
{
    Task<bool> ExistsByAccountAndStripePaymentMethodIdAsync(
        Guid accountId,
        string stripePaymentMethodId,
        CancellationToken cancellationToken);

    Task<bool> HasAnyByAccountIdAsync(
        Guid accountId,
        CancellationToken cancellationToken);

    Task<PaymentMethodEntity?> GetFallbackPaymentMethodAsync(Guid accountId, string excludedMethodId, CancellationToken cancellationToken);
}
