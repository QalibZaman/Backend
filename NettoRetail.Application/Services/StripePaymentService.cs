using System.Collections.Generic;
using Microsoft.Extensions.Logging;
using NettoRetail.Application.Exceptions;
using NettoRetail.Application.Interfaces;
using NettoRetail.Shared.Constants;
using Stripe;

namespace NettoRetail.Application.Services;

public sealed class StripePaymentService(
    CustomerService customerService,
    PaymentMethodService paymentMethodService,
    ILogger<StripePaymentService> logger) : IStripePaymentService
{

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
            logger.LogError(ex, "Stripe error while creating customer for email {email}", email);
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

            ValidateCardPaymentMethod(pm);

            return new CardSnapshot(
                pm.Id,
                pm.Card.Brand,
                pm.Card.ExpMonth,
                pm.Card.ExpYear,
                pm.Card.Last4);
        }
        catch (StripeException ex)
        {
            logger.LogError(ex, "Stripe error while attaching payment method {paymentMethodId}", paymentMethodId);
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
            logger.LogError(ex, "Stripe error while changing default payment method to {paymentMethodId}", paymentMethodId);
            throw new PaymentExceptions(ex.Message);
        }
    }

    public async Task DetachPaymentMethodAsync(string paymentMethodId)
    {
        try
        {
            await paymentMethodService.DetachAsync(paymentMethodId);
        }
        catch (StripeException ex)
        {
            logger.LogError(ex, "Stripe error while detaching payment method {PaymentMethodId}", paymentMethodId);
            throw new PaymentExceptions(ex.Message);
        }
    }

    private void ValidateCardPaymentMethod(PaymentMethod pm)
    {
        if (pm.Card is null)
        {
            throw new PaymentExceptions("Only card payment methods are supported.");
        }
    }
}
