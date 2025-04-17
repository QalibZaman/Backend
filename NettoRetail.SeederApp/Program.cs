using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using NettoRetail.DataAccess.Context;
using NettoRetail.DataAccess.Options;
using NettoRetail.DataAccess.Repositories;
using NettoRetail.SeederApp.Services;
using NettoRetail.Shared.Interfaces.Repositories;

var config = new ConfigurationBuilder()
    .SetBasePath(Directory.GetCurrentDirectory())
    .AddJsonFile("appsettings.json")
    .Build();

var services = new ServiceCollection();

services.AddSingleton<IConfiguration>(config);
services.Configure<ConnectionStrings>(config.GetSection("ConnectionStrings"));

services.AddDbContext<AppDbContext>(options =>
{
    var connectionString = config.GetConnectionString("DefaultConnection");
    options.UseNpgsql(connectionString);
});

services.AddScoped<IAccountRepository, AccountRepository>();
services.AddScoped<SuperAdminSeeder>();

var provider = services.BuildServiceProvider();
using var scope = provider.CreateScope();

var seeder = scope.ServiceProvider.GetRequiredService<SuperAdminSeeder>();
await seeder.SeedAsync();
