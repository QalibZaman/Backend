using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;
using NettoHealth.DataAccess.Options;
using NettoHealth.Domain.Entities;

namespace NettoHealth.DataAccess.Context;

public class AppDbContext(DbContextOptions options, IOptions<ConnectionStrings> connectionStringsOptions) : DbContext(options)
{
    public DbSet<HealthcareProviderEntity> HealthcareProviders { get; set; }

    protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
    {
        optionsBuilder.UseNpgsql(connectionStringsOptions.Value.DefaultConnection);
    }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.ApplyConfigurationsFromAssembly(typeof(AppDbContext).Assembly);
    }
}