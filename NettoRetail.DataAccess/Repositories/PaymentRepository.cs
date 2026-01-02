using Microsoft.EntityFrameworkCore;
using NettoRetail.DataAccess.Context;
using NettoRetail.Domain.Entities;
using NettoRetail.Shared.Interfaces.Repositories;

namespace NettoRetail.DataAccess.Repositories
{
    public class PaymentRepository(AppDbContext dbContext) : BaseRepository<PaymentMethodEntity>(dbContext), IPaymentRepository
    {
        public async Task<bool> ExistsByStripePaymentMethodIdAsync(
    string stripePaymentMethodId,
    CancellationToken cancellationToken)
        {
            return await dbContext.Set<PaymentMethodEntity>()
                .AnyAsync(p => p.StripePaymentMethodId == stripePaymentMethodId, cancellationToken);
        }
    }
}
