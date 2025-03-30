using NettoRetail.Domain.Entities;
using NettoRetail.Shared.Specifications;

namespace NettoRetail.Shared.Interfaces.Repositories;

public interface ICustomerRepository : IBaseRepository<CustomerEntity>
{
    Task<ICollection<CustomerEntity>> GetCustomersPaginatedAsync(Specification<CustomerEntity> specification, CancellationToken cancellationToken);
}