using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;
using NettoRetail.DataAccess.Options;
using NettoRetail.Domain.Entities;
using NettoRetail.Domain.Entities.JunctionEntities;

namespace NettoRetail.DataAccess.Context;

public class AppDbContext(DbContextOptions<AppDbContext> options, IOptions<ConnectionStrings> connectionStringsOptions) : DbContext(options)
{
    public DbSet<AccountEntity> Accounts { get; set; }
    public DbSet<RoleEntity> Roles { get; set; }
    public DbSet<FailedAuthAttemptEntity> FailedAuthAttempt { get; set; }
    public DbSet<CategoryEntity> Categories { get; set; }
    public DbSet<SessionEntity> Sessions { get; set; }
    public DbSet<ShopEntity> Shops { get; set; }
    public DbSet<CartEntity> Carts { get; set; }
    public DbSet<CartItemEntity> CartItems { get; set; }
    public DbSet<ContentEntity> Contents { get; set; }
    public DbSet<CountryEntity> Countries { get; set; }
    public DbSet<InventoryItemEntity> InventoryItems { get; set; }
    public DbSet<InventoryItemImageEntity> InventoryItemImages { get; set; }
    public DbSet<InventoryItemBannerEntity> InventoryItemBanners { get; set; }
    public DbSet<BlacklistedTokenEntity> BlacklistedTokens { get; set; }
    public DbSet<CustomerEntity> Customers { get; set; }
    public DbSet<OrderEntity> Orders { get; set; }
    public DbSet<OrderItemEntity> Order_Item { get; set; }
    public DbSet<OrderStatusHistoryEntity> OrderStatusHistories { get; set; }
    public DbSet<OfferEntity> OfferSections { get; set; }
    public DbSet<OffersProductEntity> OffersProducts { get; set; }
    public DbSet<BrandEntity> Brands { get; set; }
    public DbSet<ReviewEntity> Reviews { get; set; }
    public DbSet<PaymentMethodEntity> PaymentMethods { get; set; }
    public DbSet<SubscriptionPlanEntity> SubscriptionPlans { get; set; }
    public DbSet<PasswordHistoryEntity> PasswordHistories { get; set; }
    public DbSet<FeatureEntity> Features { get; set; }
    public DbSet<PriceHistoryEntity> PriceHistories { get; set; }
    public DbSet<PaymentHistoryEntity> PaymentHistories { get; set; }
    public DbSet<FeedbackEntity> Feedbacks { get; set; }
    public DbSet<TeaserSectionEntity> TeaserSection { get; set; }
    public DbSet<NewProductsSectionEntity> NewProductsSections { get; set; }
    public DbSet<NewProductsProductEntity> NewProductsProducts { get; set; }
    public DbSet<FavouriteListItemEntity> FavouriteListItems { get; set; }
    public DbSet<ComparisonListEntity> ComparisonLists { get; set; }
    public DbSet<ComparisonListItemEntity> ComparisonListItems { get; set; }

    protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
    {
    }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.ApplyConfigurationsFromAssembly(typeof(AppDbContext).Assembly);
    }
}