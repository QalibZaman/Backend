using Microsoft.EntityFrameworkCore;
using NettoRetail.DataAccess.Context;
using NettoRetail.Domain.Entities;
using NettoRetail.Shared.Interfaces.Repositories;

namespace NettoRetail.DataAccess.Repositories;

public class PlatfromAdminRepository(AppDbContext dbContext) : BaseRepository<PlatformAdminEntity>(dbContext), IPlatformAdminRepository
{
}
