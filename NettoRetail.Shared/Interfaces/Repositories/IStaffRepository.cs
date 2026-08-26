using NettoRetail.Domain.Entities;
using NettoRetail.Shared.Specifications;

namespace NettoRetail.Shared.Interfaces.Repositories;

public interface IStaffRepository : IBaseRepository<StaffEntity>
{
    Task<IEnumerable<string>> GetAccountEmailsAsync(Specification<StaffEntity> specifications, CancellationToken cancellationToken);
    Task<IEnumerable<StaffEntity>> GetStaffAccounts(Specification<StaffEntity> specification, CancellationToken cancellationToken);

    Task<(ICollection<StaffEntity> Items, int TotalCount)> GetStaffAccountsPaginated(
        Specification<StaffEntity> specification,
        CancellationToken cancellationToken);
}
