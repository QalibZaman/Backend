using NettoRetail.Domain.Entities;
using NettoRetail.Shared.Specifications;

namespace NettoRetail.Shared.Interfaces.Repositories;

public interface IAccountRepository : IBaseRepository<AccountEntity>
{
    Task<IEnumerable<string>> GetAccountEmailsAsync(Specification<AccountEntity> specifications, CancellationToken cancellationToken);
    Task<IEnumerable<AccountEntity>> GetStaffAccounts(Specification<AccountEntity> specifications, CancellationToken cancellationToken);
}