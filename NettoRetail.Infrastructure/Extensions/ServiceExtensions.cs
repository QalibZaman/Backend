using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using NettoHealth.Infrastructure.Services;
using NettoRetail.Infrastructure.Interfaces;
using NettoRetail.Infrastructure.Options;
using NettoRetail.Infrastructure.Services;
using Stripe;

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
        services.Configure<StripeOptions>(configuration.GetSection(nameof(StripeOptions)));
        StripeConfiguration.ApiKey = configuration.GetSection(nameof(StripeOptions))["SecretKey"];

        services.AddScoped<IEmailSender, EmailSender>();
        services.AddScoped<IEmailTemplateService, EmailTemplateService>();
        return services;
    }
}