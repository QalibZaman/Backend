using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using NettoRetail.DataAccess.Context;
using NettoRetail.DataAccess.Options;

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

