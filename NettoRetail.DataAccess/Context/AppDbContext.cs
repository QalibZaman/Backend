using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;
using NettoRetail.DataAccess.Options;
using NettoRetail.Domain.Entities;

namespace NettoRetail.DataAccess.Context;

public class AppDbContext(DbContextOptions options, IOptions<ConnectionStrings> connectionStringsOptions) : DbContext(options)
{
    public DbSet<AccountEntity> Accounts { get; set; }
    public DbSet<RoleEntity> Roles { get; set; }
    public DbSet<FailedAuthAttemptEntity> FailedAuthAttempt { get; set; }
    public DbSet<CategoryEntity> Categories { get; set; }
    public DbSet<SessionEntity> Sessions { get; set; }
    public DbSet<ShopEntity> Shops { get; set; }
    public DbSet<CountryEntity> Countries { get; set; }
    public DbSet<InventoryItemEntity> InventoryItems { get; set; }
    public DbSet<BlacklistedTokenEntity> BlacklistedTokens { get; set; }

    protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
    {
        optionsBuilder.UseNpgsql(connectionStringsOptions.Value.DefaultConnection);
    }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.ApplyConfigurationsFromAssembly(typeof(AppDbContext).Assembly);
    }
}