using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;
using NettoRetail.DataAccess.Identity;
using NettoRetail.DataAccess.Options;
using NettoRetail.Domain.Entities;

namespace NettoRetail.DataAccess.Context;

public class AppDbContext(
    DbContextOptions<AppDbContext> options)
    : IdentityDbContext<ApplicationUser, IdentityRole<Guid>, Guid>(options)
{
    public DbSet<BlacklistedTokenEntity> BlacklistedTokens { get; set; }
    public DbSet<StaffEntity> Staff { get; set; }
    public DbSet<RoleEntity> ApplicationRoles { get; set; }
    public DbSet<CommunicationPreferenceEntity> CommunicationPreferences { get; set; }
    public DbSet<CustomerEntity> Customers { get; set; }
    public DbSet<SessionEntity> Sessions { get; set; }
    protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
    {
    }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        // IMPORTANT: call the base implementation first
        base.OnModelCreating(modelBuilder);
        modelBuilder.Entity<BaseUserEntity>().UseTpcMappingStrategy();

        modelBuilder.Entity<StaffEntity>().ToTable("Staff");
        modelBuilder.Entity<CustomerEntity>().ToTable("Customers");

        modelBuilder.ApplyConfigurationsFromAssembly(typeof(AppDbContext).Assembly);
    }


}
