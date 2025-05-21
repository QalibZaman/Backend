using NettoRetail.Domain.Entities;
using NettoRetail.Shared.Specifications;

namespace NettoRetail.Shared.Interfaces.Repositories;

public interface IAccountRepository : IBaseRepository<AccountEntity>
{
    Task<IEnumerable<string>> GetAccountEmailsAsync(Specification<AccountEntity> specifications, CancellationToken cancellationToken);
    Task<IEnumerable<AccountEntity>> GetStaffAccounts(Specification<AccountEntity> specification, CancellationToken cancellationToken);

    Task<(ICollection<AccountEntity> Items, int TotalCount)> GetStaffAccountsPaginated(
        Specification<AccountEntity> specification,
        CancellationToken cancellationToken);
}