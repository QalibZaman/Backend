using NettoRetail.API.Infrastructure.Extensions;
using NettoRetail.Application.Extensions;
using NettoRetail.DataAccess.Extensions;
using NettoRetail.Infrastructure.Extensions;
using NettoRetail.Shared.Extensions;
using Serilog;

var builder = WebApplication.CreateBuilder(args);

builder.Host.UseSerilog((context, configuration) =>
    configuration.ReadFrom.Configuration(context.Configuration));

builder.Services.ConfigureSwagger();
builder.Services.ConfigureAuthentication();
builder.Services.ConfigureValidation();
builder.Services.AddRouting(options => options.LowercaseUrls = true);

builder.Services.AddApplicationServiceExtensions();
builder.Services.AddDataAccessServiceExtensions(builder.Configuration);
builder.Services.AddSharedServiceExtensions(builder.Configuration);
builder.Services.AddInfrastructureExtensions(builder.Configuration);

builder.Services.AddCors(options => options.AddDefaultPolicy(policy =>
{
    policy.AllowAnyOrigin()
        .AllowAnyHeader()
        .AllowAnyMethod();
}));

//For countries
builder.Services.AddMemoryCache();

var app = builder.Build();

app.Migrate();
app.UseTracing();
app.UseSerilogRequestLogging();
app.UseGlobalExceptionHandling();
app.UseConfiguredSwagger();
app.UseCors();
app.UseAuthentication();
app.UseAuthorization();
app.UseSessionActivity();
app.MapControllers();

app.Run();

public partial class Program { }