using System.Reflection;
using EntityFramework.Exceptions.PostgreSQL;
using Hangfire;
using Hangfire.PostgreSql;
using Mapster;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using NettoRetail.Application.Interfaces;
using NettoRetail.Application.Services;
using NettoRetail.Application.Services.FavouriteListItems;
using NettoRetail.Application.Services.IBlobNameGenerator;
using NettoRetail.Application.Services.IBrandService;
using NettoRetail.Application.Services.ICartCalculatorService;
using NettoRetail.Application.Services.ICartService;
using NettoRetail.Application.Services.ICategoryService;
using NettoRetail.Application.Services.ICountryCacheService;
using NettoRetail.Application.Services.ICountryValidationService;
using NettoRetail.Application.Services.IInventoryItemQueryService;
using NettoRetail.Application.Services.IInventoryService;
using NettoRetail.Application.Services.ILinkService;
using NettoRetail.Application.Services.ImageUploadService;
using NettoRetail.Application.Services.INewsletterService;
using NettoRetail.Application.Services.INotificationService;
using NettoRetail.Application.Services.IOrderService;
using NettoRetail.Application.Services.IOrderService.OrderStatusService;
using NettoRetail.Application.Services.IPasswordService;
using NettoRetail.Application.Services.IPdfGenerator;
using NettoRetail.Application.Services.IRecentlyViewedService;
using NettoRetail.Application.Services.IRoleService;
using NettoRetail.Application.Services.ISessionCleanupService;
using NettoRetail.Application.Services.ISessionModelFactory;
using NettoRetail.Application.Services.IShopValidationService;
using NettoRetail.Application.Services.IStaffService;
using NettoRetail.Application.Services.PermissionCache;
using NettoRetail.Application.Services.VariantMediaService;
using NettoRetail.Application.Services.VariationService;
using NettoRetail.Application.Services.VariationService.VariationDetailsService;
using NettoRetail.Application.Utilities;
using NettoRetail.DataAccess.Context;
using NettoRetail.DataAccess.Options;
using NettoRetail.Infrastructure.Services;
using NettoRetail.Shared.Options;
using Stripe;

namespace NettoRetail.Application.Extensions;

public static class ServiceExtensions
{
    public static void AddApplicationServiceExtensions(this IServiceCollection services, IConfiguration configuration)
    {
        services.AddDbContext<AppDbContext>(options =>
        {
            var connectionString = configuration.GetSection(nameof(ConnectionStrings)).GetValue<string>("DefaultConnection");
            options.UseNpgsql(connectionString)
                .UseExceptionProcessor();
        });

        // Mediator
        services.AddMediatR(config => config.RegisterServicesFromAssembly(Assembly.GetExecutingAssembly()));

        //Mapping
        services.AddMapster();
        TypeAdapterConfig.GlobalSettings.Apply(new MappingConfiguration.MappingConfiguration());

        //Background
        services.AddHostedService<SessionCleanupService>();
        services.AddHostedService<TokenCleanupService>();

        //Hangfire
        services.AddHangfire(config =>
        {
            var connectionString = configuration.GetSection(nameof(ConnectionStrings)).GetValue<string>("HangfireConnection");

            config.UseSimpleAssemblyNameTypeSerializer()
                .UseRecommendedSerializerSettings()
                .UsePostgreSqlStorage(options =>
                {
                    options.UseNpgsqlConnection(connectionString);
                });
        });
        services.AddHangfireServer();

        //Services
        services.AddScoped<IStripePaymentService, StripePaymentService>();
        services.AddScoped<ILinkService, LinkService>();
        services.AddScoped<IInvitationTokenService, InvitationTokenService>();
        services.AddSingleton(TypeAdapterConfig.GlobalSettings);
        services.AddScoped<IFailedAuthAttemptService, FailedAuthAttemptService>();
        services.AddScoped<ISessionModelFactory, SessionModelFactory>();
        services.AddSingleton<IPermissionCacheService, PermissionCacheService>();
        services.AddScoped<ICountryDetailsValidationService, CountryDetailsValidationService>();
        services.AddScoped<ICategoryLevelManagementService, CategoryLevelManagementService>();
        services.AddScoped<ICartService, CartService>();
        services.AddScoped<ICartCalculatorService, CartCalculatorService>();
        services.AddScoped<IRecentlyViewedService, RecentlyViewedService>();
        services.AddScoped<ICategoryService, CategoryService>();
        services.AddScoped<IJwtManager, JwtManager>();
        services.AddScoped<IBlacklistTokenService, BlacklistTokenService>();
        services.AddScoped<ITotpService, TotpService>();
        services.AddScoped<ICsvFileService, CsvFileService>();
        services.AddScoped<IEmailBuilderService, EmailBuilderService>();
        services.AddScoped<IInventoryItemQueryService, InventoryItemQueryService>();
        services.AddScoped<ISessionTerminationService, SessionTerminationService>();
        services.AddScoped<IPasswordService, PasswordService>();
        services.AddScoped<PaymentMethodService>();
        services.AddScoped<CustomerService>();
        services.AddScoped<IPdfGenerator, PdfGenerator>();
        services.AddScoped<INewsletterService, NewsletterService>();
        services.AddScoped<IPriceHistoryService, PriceHistoryService>();
        services.AddScoped<ISubscriptionNotificationService, SubscriptionNotificationService>();
        services.AddScoped<INotificationService, NotificationService>();
        services.AddScoped<IImageUploadService, ImageUploadService>();
        services.AddScoped<IShopDetailsValidationService, ShopDetailsValidationService>();
        services.AddScoped<IInventoryService, InventoryService>();
        services.AddScoped<IBrandService, BrandService>();
        services.AddScoped<IOrderService, OrderService>();
        services.AddScoped<IFavouriteListItemsService, FavouriteListItemsService>();
        services.AddScoped<IStaffService, StaffService>();
        services.AddScoped<IFilteredOrderService, FilteredOrderService>();
        services.AddScoped<IShopSubscriptionService, ShopSubscriptionService>();
        services.AddScoped<IOrderStatusService, OrderStatusService>();
        services.AddScoped<IRoleService, RoleService>();
        services.AddScoped<IStripePaymentService, StripePaymentService>();
        services.AddScoped<ILinkService, LinkService>();
        services.AddScoped<IVariationService, VariationService>();
        services.AddScoped<IVariantMediaService, VariantMediaService>();
        services.AddScoped<IVariationDetailsService, VariationDetailsService>();

        var featureToggleOptions = configuration
            .GetSection(nameof(FeatureToggleOptions))
            .Get<FeatureToggleOptions>();

        if (featureToggleOptions != null && featureToggleOptions.Redis)
        {
            services.AddSingleton<ICountryCacheService, RedisCountryCacheService>();
        }
        else
        {
            services.AddSingleton<ICountryCacheService, CountryCacheService>();
        }

        services.AddScoped<IBlobNameGenerator, BlobNameGenerator>();
        services.AddScoped<ChargeService>();
    }
}