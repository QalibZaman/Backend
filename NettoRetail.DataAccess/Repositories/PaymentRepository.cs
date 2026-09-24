using Microsoft.EntityFrameworkCore;
using NettoRetail.DataAccess.Context;
using NettoRetail.Domain.Entities;
using NettoRetail.Shared.Interfaces.Repositories;

namespace NettoRetail.DataAccess.Repositories;

public class PaymentRepository(AppDbContext dbContext)
    : BaseRepository<PaymentMethodEntity>(dbContext), IPaymentRepository
{
    public async Task<bool> HasAnyByAccountIdAsync(Guid accountId, CancellationToken cancellationToken)
    {
        return await _dbSet.AnyAsync(p => p.AccountId == accountId, cancellationToken);
    }

    public async Task<bool> ExistsDuplicateCardAsync(
        Guid accountId,
        string cardBrand,
        string fourLastCardNumbers,
        long cardExpMonth,
        long cardExpYear,
        CancellationToken cancellationToken)
    {
        return await _dbSet.AnyAsync(
            p =>
            p.AccountId == accountId &&
            p.CardBrand == cardBrand &&
            p.FourLastCardNumbers == fourLastCardNumbers &&
            p.CardExpMonth == cardExpMonth &&
            p.CardExpYear == cardExpYear,
            cancellationToken);
    }
}