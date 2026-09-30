using Microsoft.Extensions.Logging;
using NettoRetail.Application.Exceptions;
using NettoRetail.Application.Interfaces;
using Stripe;

namespace NettoRetail.Application.Services;

public sealed class StripePaymentService(
    CustomerService customerService,
    PaymentMethodService paymentMethodService,
    SubscriptionService subscriptionService,
    ILogger<StripePaymentService> logger) : IStripePaymentService
{
    private const string ActiveSubscriptionStatus = "active";

    public async Task<string> EnsureCustomerAsync(
        string? existingCustomerId,
        string fullName,
        string email,
        CancellationToken cancellationToken)
    {
        if (!string.IsNullOrWhiteSpace(existingCustomerId))
            return existingCustomerId;

        var options = new CustomerCreateOptions
        {
            Name = fullName,
            Email = email
        };

        try
        {
            var customer = await customerService.CreateAsync(options, cancellationToken: cancellationToken);
            return customer.Id;
        }
        catch (StripeException ex)
        {
            logger.LogError(ex, "Stripe error while creating customer for email {Email}", email);
            throw new PaymentExceptions(ex.Message);
        }
    }

    public async Task<CardSnapshot> AttachAndGetCardAsync(
        string paymentMethodId,
        string customerId,
        CancellationToken cancellationToken)
    {
        try
        {
            var pm = await paymentMethodService.AttachAsync(
                paymentMethodId,
                new PaymentMethodAttachOptions { Customer = customerId },
                cancellationToken: cancellationToken);

            if (pm.Card is null)
            {
                throw new PaymentExceptions("Only card payment methods are supported.");
            }

            return new CardSnapshot(
                pm.Id,
                pm.Card.Brand,
                pm.Card.ExpMonth,
                pm.Card.ExpYear,
                pm.Card.Last4);
        }
        catch (StripeException ex)
        {
            logger.LogError(ex, "Stripe error while attaching payment method {PaymentMethodId}", paymentMethodId);
            throw new PaymentExceptions(ex.Message);
        }
    }

    public async Task SetDefaultPaymentMethodAsync(
        string customerId,
        string paymentMethodId,
        CancellationToken cancellationToken)
    {
        var options = new CustomerUpdateOptions
        {
            InvoiceSettings = new CustomerInvoiceSettingsOptions
            {
                DefaultPaymentMethod = paymentMethodId
            }
        };

        try
        {
            await customerService.UpdateAsync(customerId, options, cancellationToken: cancellationToken);
        }
        catch (StripeException ex)
        {
            logger.LogError(ex, "Stripe error while changing default payment method to {PaymentMethodId}", paymentMethodId);
            throw new PaymentExceptions(ex.Message);
        }
    }

    public async Task<SubscriptionSnapshot> GetSubscriptionAsync(
    string subscriptionId,
    CancellationToken cancellationToken)
    {
        try
        {
            var subscription = await subscriptionService.GetAsync(subscriptionId, cancellationToken: cancellationToken);

            var periodEnd = subscription.Items.Data.FirstOrDefault()?.CurrentPeriodEnd
                ?? throw new PaymentExceptions("Subscription has no items.");

            return new SubscriptionSnapshot(
                IsRenewing: subscription.Status == ActiveSubscriptionStatus && !subscription.CancelAtPeriodEnd,
                CurrentPeriodEnd: periodEnd);
        }
        catch (StripeException ex)
        {
            logger.LogError(ex, "Stripe error while getting subscription {SubscriptionId}", subscriptionId);
            throw new PaymentExceptions(ex.Message);
        }
    }

    public async Task SetSubscriptionCancelAtPeriodEndAsync(
        string subscriptionId,
        bool cancelAtPeriodEnd,
        CancellationToken cancellationToken)
    {
        var options = new SubscriptionUpdateOptions { CancelAtPeriodEnd = cancelAtPeriodEnd };

        try
        {
            await subscriptionService.UpdateAsync(subscriptionId, options, cancellationToken: cancellationToken);
        }
        catch (StripeException ex)
        {
            logger.LogError(ex, "Stripe error while setting cancel_at_period_end={Value} for subscription {SubscriptionId}", cancelAtPeriodEnd, subscriptionId);
            throw new PaymentExceptions(ex.Message);
        }
    }

    public async Task DetachPaymentMethodAsync(string paymentMethodId, CancellationToken cancellationToken)
    {
        try
        {
            await paymentMethodService.DetachAsync(paymentMethodId, cancellationToken: cancellationToken);
        }
        catch (StripeException ex)
        {
            logger.LogError(ex, "Stripe error while detaching payment method {PaymentMethodId}", paymentMethodId);
            throw new PaymentExceptions(ex.Message);
        }
    }
}