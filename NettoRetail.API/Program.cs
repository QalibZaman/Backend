using NettoRetail.API.Infrastructure.Extensions;
using NettoRetail.Application.Extensions;
using NettoRetail.DataAccess.Extensions;
using NettoRetail.Infrastructure.Extensions;
using NettoRetail.Shared.Extensions;

var builder = WebApplication.CreateBuilder(args);

builder.Services.ConfigureSwagger();
builder.Services.ConfigureAuthentication();
builder.Services.ConfigureValidation();
builder.Services.AddRouting(options => options.LowercaseUrls = true);

builder.Services.AddApplicationServiceExtensions();
builder.Services.AddDataAccessServiceExtensions(builder.Configuration);
builder.Services.AddSharedServiceExtensions(builder.Configuration);
builder.Services.AddInfrastructureExtensions(builder.Configuration);

var app = builder.Build();

app.Migrate();
app.UseTracing();
app.UseGlobalExceptionHandling();
app.UseConfiguredSwagger();
app.UseAuthentication();
app.UseAuthorization();
app.UseSessionActivity();
app.MapControllers();

app.Run();