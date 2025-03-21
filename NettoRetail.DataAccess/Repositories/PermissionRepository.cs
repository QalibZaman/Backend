using NettoRetail.DataAccess.Context;
using NettoRetail.Domain.Entities;
using NettoRetail.Shared.Interfaces.Repositories;

namespace NettoRetail.DataAccess.Repositories;

public class PermissionRepository(AppDbContext dbContext)
    : BaseRepository<PermissionEntity>(dbContext),
    IPermissionRepository;
