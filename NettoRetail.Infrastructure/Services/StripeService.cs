using Microsoft.Extensions.Options;
using NettoRetail.Infrastructure.Options;
using NettoRetail.Shared.Interfaces.Services;
using Stripe;

namespace NettoRetail.Infrastructure.Services;

public class StripeService : IStripeService
{
    private const string PaymentIncomplete = "default_incomplete";
    private const string ExpandPaymentIntent = "latest_invoice.payment_intent";

    private readonly CustomerService _customerService;
    private readonly SubscriptionService _subscriptionService;

    public StripeService(
        IOptions<StripeOptions> options,
        CustomerService customerService,
        SubscriptionService subscriptionService)
    {
        StripeConfiguration.ApiKey = options.Value.SecretKey;
        _customerService = customerService;
        _subscriptionService = subscriptionService;
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

        var customer = await _customerService.CreateAsync(customerOptions, cancellationToken: cancellationToken);
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
            PaymentBehavior = PaymentIncomplete,
            Expand = new List<string> { ExpandPaymentIntent }
        };

        var subscription = await _subscriptionService.CreateAsync(subscriptionOptions, cancellationToken: cancellationToken);
        return subscription.Id;
    }
}
