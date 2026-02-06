using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using NettoHealth.Infrastructure.Services;
using NettoRetail.Infrastructure.Interfaces;
using NettoRetail.Infrastructure.Options;
using NettoRetail.Infrastructure.Services;
using NettoRetail.Infrastructure.Services.IImageValidatorService;
using NettoRetail.Shared.Interfaces.Services;
using NettoRetail.Shared.Options;
using Stripe;

namespace NettoRetail.Infrastructure.Extensions;

public static class ServiceExtensions
{
    public static IServiceCollection AddInfrastructureExtensions(this IServiceCollection services, IConfiguration configuration)
    {
        services.AddScoped<IAzureBlobService, AzureBlobService>();
        services.AddScoped<IImageValidatorService, ImageValidatorService>();
        services.AddScoped<ILockProvider, HardLockWithTtlProviderService>();

        services.AddHttpClient<IIpInfoService, IpInfoService>(client =>
        {
            client.BaseAddress = new Uri(configuration.GetSection("IpInfo").GetValue<string>("BaseUrl") ?? "https://ipinfo.io/");
        });

        services.Configure<FrontendOptions>(configuration.GetSection(nameof(FrontendOptions)));
        services.Configure<InvitationOptions>(configuration.GetSection(nameof(InvitationOptions)));
        services.Configure<FrontendOptions>(configuration.GetSection("Frontend"));
        services.Configure<NewsletterOptions>(configuration.GetSection(nameof(NewsletterOptions)));

        services.Configure<EmailOptions>(configuration.GetSection(nameof(EmailOptions)));
        services.Configure<ResetPasswordOptions>(configuration.GetSection(nameof(ResetPasswordOptions)));
        services.Configure<EmailVerificationOptions>(configuration.GetSection(nameof(EmailVerificationOptions)));
        services.Configure<StripeOptions>(configuration.GetSection(nameof(StripeOptions)));
        StripeConfiguration.ApiKey = configuration.GetSection(nameof(StripeOptions))["SecretKey"];

        services.AddSingleton<CustomerService>();
        services.AddSingleton<SubscriptionService>();

        services.AddScoped<IEmailSender, EmailSender>();
        services.AddScoped<IEmailTemplateService, EmailTemplateService>();
        services.AddScoped<IStripeService, StripeService>();

        return services;
    }
}
