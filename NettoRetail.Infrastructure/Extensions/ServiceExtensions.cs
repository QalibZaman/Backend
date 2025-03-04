using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using NettoRetail.Infrastructure.Interfaces;
using NettoRetail.Infrastructure.Options;
using NettoRetail.Infrastructure.Services;

namespace NettoRetail.Infrastructure.Extensions;

public static class ServiceExtensions
{
    public static IServiceCollection AddInfrastructureExtensions(this IServiceCollection services, IConfiguration configuration)
    {
        services.AddScoped<IAzureBlobService, AzureBlobService>();

        services.Configure<EmailOptions>(configuration.GetSection(nameof(EmailOptions)));
        services.AddScoped<IEmailSender, EmailSender>();

        return services;
    }
}