using NettoRetail.DataAccess.Context;
using NettoRetail.Domain.Entities;
using NettoRetail.Shared.Interfaces.Repositories;

namespace NettoRetail.DataAccess.Repositories;

public class CustomerRepository(AppDbContext dbContext) : BaseRepository<CustomerEntity>(dbContext), ICustomerRepository
{
}
