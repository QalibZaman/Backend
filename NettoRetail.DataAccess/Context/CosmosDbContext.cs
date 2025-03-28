using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;
using NettoRetail.DataAccess.Options;
using NettoRetail.Domain.Entities;

namespace NettoRetail.DataAccess.Context;

public class CosmosDbContext(DbContextOptions<CosmosDbContext> options, IOptions<CosmosOptions> cosmosOptions) : DbContext(options)
{
    public DbSet<ProductDetailsEntity> ProductDetails { get; set; }

    protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
    {
        optionsBuilder.UseCosmos(
            cosmosOptions.Value.AccountEndpoint,
            cosmosOptions.Value.AccountKey,
            cosmosOptions.Value.DatabaseName);
    }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<ProductDetailsEntity>().ToContainer("ProductDetailsContainer")
            .HasPartitionKey(p => p.InventoryItemId);
    }
}
