using NettoRetail.Domain.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NettoRetail.Shared.Interfaces.Repositories
{
    public interface ICartRepository : IBaseRepository<CartEntity>
    {
        Task<CartEntity?> GetByIdAsync(Guid cartId, CancellationToken cancellationToken);
        Task<CartEntity?> GetByCustomerIdAsync(Guid customerId, CancellationToken cancellationToken);
    }
}
