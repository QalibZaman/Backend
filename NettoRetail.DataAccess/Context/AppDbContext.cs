using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using NettoRetail.DataAccess.Identity;
using NettoRetail.Domain.Entities;

namespace NettoRetail.DataAccess.Context;

public class AppDbContext(
    DbContextOptions<AppDbContext> options)
    : IdentityDbContext<ApplicationUser, IdentityRole<Guid>, Guid>(options)
{
    public DbSet<BlacklistedTokenEntity> BlacklistedTokens { get; set; }
    public DbSet<StaffEntity> Staff { get; set; }
    public DbSet<RoleEntity> ApplicationRoles { get; set; }
    public DbSet<CountryEntity> Countries { get; set; }
    public DbSet<AddressEntity> Addresses { get; set; }
    public DbSet<CommunicationPreferenceEntity> CommunicationPreferences { get; set; }
    public DbSet<CustomerEntity> Customers { get; set; }
    public DbSet<SessionEntity> Sessions { get; set; }
    public DbSet<FailedAuthAttemptEntity> FailedAuthAttempts { get; set; }
    public DbSet<ActivationEmailAttemptEntity> ActivationEmailEntity { get; set; }
    public DbSet<PasswordHistoryEntity> PasswordHistories { get; set; }
    public DbSet<NotificationEntity> Notifications { get; set; }
    public DbSet<ShopEntity> Shops { get; set; }
    public DbSet<CategoryEntity> Categories { get; set; }
    public DbSet<PaymentMethodEntity> PaymentMethods { get; set; }
    public DbSet<SubscriptionEntity> Subscriptions { get; set; }
    public DbSet<SubscriptionPlanEntity> SubscriptionPlans { get; set; }

    protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
    {
    }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        modelBuilder.Entity<BaseUserEntity>()
            .UseTpcMappingStrategy();

        modelBuilder.ApplyConfigurationsFromAssembly(typeof(AppDbContext).Assembly);
    }
}
