using Hangfire;
using NettoRetail.Application.Interfaces;

namespace NettoRetail.API.Infrastructure.Extensions;

public static class HangfireExtensions
{
    public static void RegisterRecurringJobs(this WebApplication app)
    {
        RecurringJob.AddOrUpdate<INotificationCleanupService>(
            "cleanup-notifications",
            x => x.DeleteOldNotificationsAsync(CancellationToken.None),
            Cron.Daily);
    }
}