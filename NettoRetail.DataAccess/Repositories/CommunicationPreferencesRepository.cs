using Microsoft.EntityFrameworkCore;
using NettoRetail.DataAccess.Context;
using NettoRetail.Domain.Entities;
using NettoRetail.Shared.Interfaces.Repositories;
using NettoRetail.Shared.Specifications;

namespace NettoRetail.DataAccess.Repositories;

public class CommunicationPreferencesRepository(AppDbContext dbContext)
    : BaseRepository<CommunicationPreferenceEntity>(dbContext), ICommunicationPreferencesRepository;
