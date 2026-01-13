using NettoRetail.Application.Interfaces;
using Stripe;

namespace NettoRetail.Application.Services
{
    public sealed class StripePaymentService(
        CustomerService customerService,
        PaymentMethodService paymentMethodService
    ) : IStripePaymentService
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

            var customer = await customerService.CreateAsync(options, cancellationToken: cancellationToken);
            return customer.Id;
        }

        public async Task<CardSnapshot> AttachAndGetCardAsync(
            string paymentMethodId,
            string customerId,
            CancellationToken cancellationToken)
        {
            await paymentMethodService.AttachAsync(
                paymentMethodId,
                new PaymentMethodAttachOptions { Customer = customerId },
                cancellationToken: cancellationToken);

            var pm = await paymentMethodService.GetAsync(paymentMethodId, cancellationToken: cancellationToken);

            if (pm.Card is null)
                throw new InvalidOperationException("Only card payment methods are supported.");

            return new CardSnapshot(
                pm.Card.Brand,
                pm.Card.ExpMonth,
                pm.Card.ExpYear,
                pm.Card.Last4
            );
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

            await customerService.UpdateAsync(customerId, options, cancellationToken: cancellationToken);
        }

    }
}
