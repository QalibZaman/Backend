using Microsoft.EntityFrameworkCore;
using NettoHealth.DataAccess.Context;

namespace NettoHealth.API.Infrastructure.Extensions;

public static class MigrationExtensions
{
    public static IApplicationBuilder Migrate(this WebApplication webApplication)
    {
        using var scope = webApplication.Services.CreateScope();
        var context = scope.ServiceProvider.GetRequiredService<AppDbContext>();
        context.Database.Migrate();

        return webApplication;
    }
}