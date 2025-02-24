using MapsterMapper;
using NettoRetail.DataAccess.Context;
using NettoRetail.Domain.Entities;
using NettoRetail.Shared.Interfaces.Repositories;

namespace NettoRetail.DataAccess.Repositories;

public class StaffRepository(AppDbContext dbContext)
    : BaseRepository<StaffEntity>(dbContext),
    IStaffRepository;