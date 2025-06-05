using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.DependencyInjection;
using NettoRetail.DataAccess.Seeders.Interfaces;

namespace NettoRetail.DataAccess.Extensions;

public static class DatabaseInitializerExtensions
{
    public static async Task UseDatabaseSeedingAsync(this IApplicationBuilder app)
    {
        using var scope = app.ApplicationServices.CreateScope();
        var seeders = scope.ServiceProvider.GetServices<IDatabaseSeeder>();

        foreach (var seeder in seeders)
        {
            await seeder.SeedAsync(CancellationToken.None);
        }
    }
}