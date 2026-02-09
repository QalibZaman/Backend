namespace NettoRetail.Application.Interfaces
{
    public interface IStripePaymentService
    {
        Task<string> EnsureCustomerAsync(string? existingCustomerId, string fullName, string email, CancellationToken cancellationToken);
        Task<CardSnapshot> AttachAndGetCardAsync(string paymentMethodId, string customerId, CancellationToken cancellationToken);
        Task SetDefaultPaymentMethodAsync(string customerId, string paymentMethodId, CancellationToken cancellationToken);
    }

    public sealed record CardSnapshot(
        string Brand,
        long ExpMonth,
        long ExpYear,
        string Last4
    );
}
