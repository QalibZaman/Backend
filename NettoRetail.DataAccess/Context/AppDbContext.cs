using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;
using NettoRetail.DataAccess.Options;
using NettoRetail.Domain.Entities;

namespace NettoRetail.DataAccess.Context;

public class AppDbContext(DbContextOptions options, IOptions<ConnectionStrings> connectionStringsOptions) : DbContext(options)
{
    public DbSet<StaffEntity> Staff { get; set; }
    public DbSet<RoleEntity> Roles { get; set; }

    protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
    {
        optionsBuilder.UseNpgsql(connectionStringsOptions.Value.DefaultConnection);
    }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.ApplyConfigurationsFromAssembly(typeof(AppDbContext).Assembly);
    }
}