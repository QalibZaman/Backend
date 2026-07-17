using NettoRetail.Domain.Entities;
using NettoRetail.Shared.Specifications;

namespace NettoRetail.Shared.Interfaces.Repositories;

public interface ICustomerRepository : IBaseRepository<CustomerEntity>
{
    Task<bool> ExistsActiveCustomerByAccountEmailAsync(string email, CancellationToken cancellationToken);
}