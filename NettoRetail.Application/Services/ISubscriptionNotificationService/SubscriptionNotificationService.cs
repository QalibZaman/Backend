using NettoRetail.Domain.Entities;
using NettoRetail.Infrastructure.Interfaces;
using NettoRetail.Infrastructure.Models;

namespace NettoRetail.Infrastructure.Services
{
    public class SubscriptionNotificationService(IEmailSender emailSender) : ISubscriptionNotificationService
    {
        public async Task NotifyPriceChangedAsync(SubscriptionPlanEntity plan, int oldPrice, int newPrice, DateTime effectiveDate, CancellationToken cancellationToken = default)
        {
            var emails = plan.Accounts?
                .Select(a => a.Email)
                .Where(email => !string.IsNullOrWhiteSpace(email))
                .Distinct()
                .ToList();

            if (emails == null || emails.Count == 0)
                return;

            var subject = "Subscription price change notification";
            var message = $"The price of plan \"{plan.Name}\" will change from {oldPrice} to {newPrice} on {effectiveDate:dd.MM.yyyy}";

            var bulkModel = new BulkEmailModel
            {
                EmailAddresses = emails,
                Subject = subject,
                Message = message
            };

            await emailSender.SendBulkAsync(bulkModel);
        }
    }
}
