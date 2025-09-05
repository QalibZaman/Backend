using System.Collections;
using NettoRetail.API.Infrastructure.Extensions;
using NettoRetail.Application.Extensions;
using NettoRetail.Application.Hubs.NotificationHub;
using NettoRetail.DataAccess.Extensions;
using NettoRetail.Infrastructure.Extensions;
using NettoRetail.Shared.Extensions;
using Serilog;
using StackExchange.Redis;

var builder = WebApplication.CreateBuilder(args);

//builder.Host.UseSerilog((context, configuration) =>
   // configuration.ReadFrom.Configuration(context.Configuration));

builder.Services.ConfigureSwagger();
builder.Services.ConfigureAuthentication();
builder.Services.ConfigureValidation();
builder.Services.AddRouting(options => options.LowercaseUrls = true);

builder.Services.AddApplicationServiceExtensions(builder.Configuration);
builder.Services.AddDataAccessServiceExtensions(builder.Configuration);
builder.Services.AddSharedServiceExtensions(builder.Configuration);
builder.Services.AddInfrastructureExtensions(builder.Configuration);

builder.Services.AddCors(options =>
{
    options.AddPolicy(
        "CorsPolicy",
        policy => policy.WithOrigins("https://thankful-sea-0d7005210-develop.centralus.4.azurestaticapps.net", "http://localhost:4200")
                            .AllowAnyMethod()
                            .AllowAnyHeader()
                            .AllowCredentials());
});

builder.Services.AddMemoryCache();

builder.Services.AddStackExchangeRedisCache(options =>
{
    options.Configuration = builder.Configuration.GetConnectionString("RedisConnection");
});
builder.Services.AddSingleton<IConnectionMultiplexer>(sp =>
{
    var configuration = builder.Configuration.GetConnectionString("RedisConnection");
    return ConnectionMultiplexer.Connect(configuration!);
});

builder.Services.AddSignalR();

var app = builder.Build();

if (app.Environment.IsDevelopment() || app.Environment.IsEnvironment("Local"))
{
    app.Migrate();
}

await app.UseDatabaseSeedingAsync();

app.UseCors("CorsPolicy");
app.UseTracing();
//app.UseSerilogRequestLogging();
app.UseGlobalExceptionHandling();
app.UseConfiguredSwagger();
app.UseAuthentication();
app.UseAuthorization();
app.UseSessionActivity();
app.UsePermissionHandling();
app.MapControllers();

app.MapHub<NotificationHub>("/notifications");

app.Run();

public partial class Program { }