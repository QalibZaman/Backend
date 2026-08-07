using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using NettoHealth.Infrastructure.Services;
using NettoRetail.DataAccess.Context;
using NettoRetail.DataAccess.Options;
using NettoRetail.Infrastructure.Interfaces;
using NettoRetail.Infrastructure.Options;
using NettoRetail.Infrastructure.Services;
using NettoRetail.Infrastructure.Services.IImageValidatorService;
using NettoRetail.Shared.Interfaces.Services;
using Stripe;

namespace NettoRetail.Infrastructure.Extensions;

public static class ServiceExtensions
{
    public static IServiceCollection AddInfrastructureExtensions(this IServiceCollection services, IConfiguration configuration)
    {

        services.Configure<ConnectionStrings>(configuration.GetSection(nameof(ConnectionStrings)));
        services.Configure<CosmosOptions>(configuration.GetSection(nameof(CosmosOptions)));
        services.Configure<AzureBlobStorageOptions>(configuration.GetSection(nameof(AzureBlobStorageOptions)));
        
        // services.AddScoped<IAzureBlobService, AzureBlobService>();
        services.AddScoped<IImageValidatorService, ImageValidatorService>();

        services.AddHttpClient<IIpInfoService, IpInfoService>(client =>
        {
            client.BaseAddress = new Uri(configuration.GetSection("IpInfo").GetValue<string>("BaseUrl") ?? "https://ipinfo.io/");
        });

        services.Configure<PlatformAdminActivationOptions>(configuration.GetSection(nameof(PlatformAdminActivationOptions)));
        services.Configure<FrontendOptions>(configuration.GetSection("Frontend"));
        services.Configure<CustomerPasswordResetOptions>(configuration.GetSection(nameof(CustomerPasswordResetOptions)));
        services.Configure<CorsOptions>(configuration.GetSection(nameof(CorsOptions)));
        services.Configure<EmailOptions>(configuration.GetSection(nameof(EmailOptions)));
        services.Configure<ResetPasswordOptions>(configuration.GetSection(nameof(ResetPasswordOptions)));
        services.Configure<EmailVerificationOptions>(configuration.GetSection(nameof(EmailVerificationOptions)));
        services.Configure<CustomerEmailChangeOptions>(configuration.GetSection(nameof(CustomerEmailChangeOptions)));
        services.Configure<StripeOptions>(configuration.GetSection(nameof(StripeOptions)));
        StripeConfiguration.ApiKey = configuration.GetSection(nameof(StripeOptions))["SecretKey"];

        services.AddSingleton<CustomerService>();
        services.AddSingleton<SubscriptionService>();

        services.AddScoped<IEmailSender, EmailSender>();
        services.AddScoped<IEmailTemplateService, EmailTemplateService>();
        // services.AddScoped<IStripeService, StripeService>();
        services.AddDbContext<AppDbContext>();
        return services;
    }
}
