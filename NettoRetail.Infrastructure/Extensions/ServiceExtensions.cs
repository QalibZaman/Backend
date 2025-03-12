using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using NettoHealth.Infrastructure.Services;
using NettoRetail.Infrastructure.Interfaces;
using NettoRetail.Infrastructure.Options;
using NettoRetail.Infrastructure.Services;

namespace NettoRetail.Infrastructure.Extensions;

public static class ServiceExtensions
{
    public static IServiceCollection AddInfrastructureExtensions(this IServiceCollection services, IConfiguration configuration)
    {
        services.AddScoped<IAzureBlobService, AzureBlobService>();

        services.AddHttpClient<IIpInfoService, IpInfoService>(client =>
        {
            client.BaseAddress = new Uri(configuration.GetSection("IpInfo").GetValue<string>("BaseUrl") ?? "https://ipinfo.io/");
        });

        services.Configure<EmailOptions>(configuration.GetSection(nameof(EmailOptions)));
        services.Configure<ResetPasswordOptions>(configuration.GetSection(nameof(ResetPasswordOptions)));
        services.Configure<EmailTemplateOptions>(configuration.GetSection("EmailTemplateOptions"));

        services.AddScoped<IEmailSender, EmailSender>();
        services.AddScoped<IEmailTemplateService, EmailTemplateService>();
        return services;
    }
}