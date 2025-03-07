using NettoRetail.DataAccess.Context;
using NettoRetail.Domain.Entities;
using NettoRetail.Shared.Interfaces.Repositories;

namespace NettoRetail.DataAccess.Repositories;

public class InventoryItemRepository(AppDbContext dbContext)
    : BaseRepository<InventoryItemEntity>(dbContext),
        IInventoryItemRepository;