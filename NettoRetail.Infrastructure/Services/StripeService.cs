using Microsoft.Extensions.Options;
using NettoRetail.Infrastructure.Options;
using NettoRetail.Shared.Interfaces.Services;
using Stripe;

namespace NettoRetail.Infrastructure.Services;

public class StripeService : IStripeService
{
    private readonly CustomerService customerService = new();
    private readonly SubscriptionService subscriptionService = new();
    private readonly PaymentMethodService paymentMethodService = new();

    public StripeService(IOptions<StripeOptions> options)
    {
        StripeConfiguration.ApiKey = options.Value.SecretKey;
    }

    public async Task<string> CreateCustomerAsync(string email, string paymentMethodId, CancellationToken cancellationToken = default)
    {
        var customerOptions = new CustomerCreateOptions
        {
            Email = email,
            PaymentMethod = paymentMethodId,
            InvoiceSettings = new CustomerInvoiceSettingsOptions
            {
                DefaultPaymentMethod = paymentMethodId
            }
        };

        var customer = await customerService.CreateAsync(customerOptions, cancellationToken: cancellationToken);
        return customer.Id;
    }

    public async Task<string> CreateSubscriptionAsync(string customerId, string priceId, CancellationToken cancellationToken = default)
    {
        var subscriptionOptions = new SubscriptionCreateOptions
        {
            Customer = customerId,
            Items = new List<SubscriptionItemOptions>
            {
                new() { Price = priceId }
            },
            PaymentBehavior = "default_incomplete",
            Expand = new List<string> { "latest_invoice.payment_intent" }
        };

        var subscription = await subscriptionService.CreateAsync(subscriptionOptions, cancellationToken: cancellationToken);
        return subscription.Id;
    }
}
