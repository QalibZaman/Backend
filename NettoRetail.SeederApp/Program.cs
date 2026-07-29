using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using NettoRetail.DataAccess.Context;
using NettoRetail.DataAccess.Identity;
using NettoRetail.DataAccess.Options;
using NettoRetail.DataAccess.Repositories;
using NettoRetail.SeederApp.Services;
using NettoRetail.Shared.Interfaces.Repositories;

var config = new ConfigurationBuilder()
    .SetBasePath(Directory.GetCurrentDirectory())
    .AddJsonFile("appsettings.json")
    .Build();

var services = new ServiceCollection();
services.AddLogging();
services.AddSingleton<IConfiguration>(config);

services.Configure<ConnectionStrings>(config.GetSection("ConnectionStrings"));
services
    .AddIdentity<ApplicationUser, IdentityRole<Guid>>()
    .AddEntityFrameworkStores<AppDbContext>()
    .AddDefaultTokenProviders();
services.AddDbContext<AppDbContext>(options =>
{
    var connectionString = config.GetConnectionString("DefaultConnection");
    options.UseNpgsql(connectionString);
});

services.AddScoped<IStaffRepository, StaffRepository>();
var profile = config.GetValue<string>("Profile");

if (profile == "RoleSeeder")
{
    services.AddScoped<RoleSeeder>();

    var provider = services.BuildServiceProvider();
    using var scope = provider.CreateScope();

    var seeder = scope.ServiceProvider.GetRequiredService<RoleSeeder>();
    await seeder.SeedAsync();
}
else
{
    Console.WriteLine($"No seeder matched for profile: {profile}");
}

