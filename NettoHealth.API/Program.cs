using NettoHealth.API.Infrastructure.Extensions;
using NettoHealth.Application.Extensions;
using NettoHealth.DataAccess.Extensions;
using NettoHealth.Infrastructure.Extensions;
using NettoHealth.Shared.Extensions;

var builder = WebApplication.CreateBuilder(args);

builder.Services.ConfigureSwagger();
builder.Services.ConfigureAuthentication();
builder.Services.ConfigureValidation();
builder.Services.AddRouting(options => options.LowercaseUrls = true);

builder.Services.AddApplicationServiceExtensions();
builder.Services.AddDataAccessServiceExtensions(builder.Configuration);
builder.Services.AddSharedServiceExtensions(builder.Configuration);
builder.Services.AddInfrastructureExtensions();

var app = builder.Build();

app.Migrate();
app.UseTracing();
app.UseGlobalExceptionHandling();
app.UseConfiguredSwagger();
app.UseAuthentication();
app.UseAuthorization();
app.MapControllers();

app.Run();