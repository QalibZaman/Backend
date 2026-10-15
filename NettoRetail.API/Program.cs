using Hangfire;
using Microsoft.AspNetCore.HttpOverrides;
using NettoRetail.API.Infrastructure.Extensions;
using NettoRetail.Application.Extensions;
using NettoRetail.Application.Hubs.NotificationHub;
using NettoRetail.Application.Interfaces;
using NettoRetail.Application.Interfaces.Services.UserService;
using NettoRetail.DataAccess.Extensions;
using NettoRetail.DataAccess.Repositories;
using NettoRetail.Infrastructure.Extensions;
using NettoRetail.Infrastructure.Interfaces;
using NettoRetail.Infrastructure.Services;
using NettoRetail.Infrastructure.Services.IImageValidatorService;
using NettoRetail.Shared.Extensions;
using NettoRetail.Shared.Interfaces.Repositories;

var builder = WebApplication.CreateBuilder(args);

//builder.Host.UseSerilog((context, configuration) =>
//    configuration.ReadFrom.Configuration(context.Configuration));

builder.Services.ConfigureSwagger();

builder.Services.ConfigureAuthentication();

builder.Services.ConfigureValidation();

builder.Services.AddRouting(options => options.LowercaseUrls = true);

builder.Services.AddInfrastructureExtensions(builder.Configuration);

builder.Services.AddApplicationServiceExtensions(builder.Configuration);

builder.Services.AddDataAccessServiceExtensions(builder.Configuration);

builder.Services.AddSharedServiceExtensions(builder.Configuration);

builder.Services.AddScoped<IAzureBlobService, AzureBlobService>();

builder.Services.AddScoped<IImageValidatorService, ImageValidatorService>();

builder.Services.AddCors(options =>
{
    var allowedOrigins = builder.Configuration.GetSection("CorsOptions:AllowedOrigins").Get<string[]>()
        ?? throw new InvalidOperationException(
            "Configuration section 'CorsOptions:AllowedOrigins' is missing or empty.");

    options.AddPolicy("CorsPolicy", policy =>
    {
        policy.WithOrigins(allowedOrigins!)
              .AllowAnyHeader()
              .AllowAnyMethod()
              .AllowCredentials();
    });
});

builder.Services.AddScoped<IUserService, UserService>();

builder.Services.AddScoped<IRoleRepository, RoleRepository>();
builder.Services.AddScoped<ICountryRepository, CountryRepository>();

builder.Services.AddScoped<IShopLinkRepository, ShopLinkRepository>();

builder.Services.AddMemoryCache();

builder.Services.AddHttpContextAccessor();

builder.Services.AddStackExchangeRedisCache(options =>
{
    options.Configuration = builder.Configuration.GetConnectionString("RedisConnection");
});

builder.Services.AddSignalR();

builder.Services.Configure<ForwardedHeadersOptions>(options =>
{
    options.ForwardedHeaders = ForwardedHeaders.XForwardedFor | ForwardedHeaders.XForwardedProto;
    options.KnownNetworks.Clear();
    options.KnownProxies.Clear();
});

var app = builder.Build();

app.UseForwardedHeaders();

if (app.Environment.IsDevelopment() || app.Environment.IsEnvironment("Local"))
{
    app.Migrate();
}

await app.UseDatabaseSeedingAsync();

app.UseCors("CorsPolicy");

//app.UseSerilogRequestLogging();

app.UseGlobalExceptionHandling();

app.UseConfiguredSwagger();

app.UseAuthentication();

app.UseAuthorization();

app.MapControllers();

app.UseHangfireDashboard();

app.RegisterRecurringJobs();

app.MapHub<NotificationHub>("/notifications");

app.Run();

public partial class Program { }