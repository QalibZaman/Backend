using NettoRetail.Domain.Entities;

namespace NettoRetail.Shared.Interfaces.Repositories;

public interface IPaymentRepository : IBaseRepository<PaymentMethodEntity>
{
    Task<bool> ExistsByStripePaymentMethodIdAsync(
    string stripePaymentMethodId,
    CancellationToken cancellationToken);
    Task<PaymentMethodEntity?> GetFallbackPaymentMethodAsync(Guid accountId, string excludedMethodId, CancellationToken cancellationToken);
}
