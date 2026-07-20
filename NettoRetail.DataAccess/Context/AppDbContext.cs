using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;
using NettoRetail.DataAccess.Options;

namespace NettoRetail.DataAccess.Context;

public class AppDbContext(DbContextOptions<AppDbContext> options, IOptions<ConnectionStrings> connectionStringsOptions) : DbContext(options)
{
    protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
    {
    }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.ApplyConfigurationsFromAssembly(typeof(AppDbContext).Assembly);
    }
}
