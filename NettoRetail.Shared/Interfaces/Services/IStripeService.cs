namespace NettoRetail.Shared.Interfaces.Services;

public interface IStripeService
{
    Task<string> CreateCustomerAsync(string email, string paymentMethodId, CancellationToken cancellationToken);

    Task<string> CreateSubscriptionAsync(string customerId, string priceId, CancellationToken cancellationToken);
}