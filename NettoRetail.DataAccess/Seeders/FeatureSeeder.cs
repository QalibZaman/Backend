using Microsoft.EntityFrameworkCore;
using NettoRetail.DataAccess.Context;
using NettoRetail.DataAccess.Seeders.Interfaces;
using NettoRetail.Domain.Entities;

namespace NettoRetail.DataAccess.Seeders
{
    public class FeatureSeeder(AppDbContext dbContext) : IDatabaseSeeder
    {
        public async Task SeedAsync(CancellationToken cancellationToken)
        {
            if (await dbContext.Features.AnyAsync(cancellationToken))
                return;

            var features = new List<FeatureEntity>
            {
                new FeatureEntity
                {
                    Name = "Stub - Basic Functionality",
                    Description = "Provides access to core features."
                },
                new FeatureEntity
                {
                    Name = "Stub - Advanced Analytics",
                    Description = "Enables advanced reporting and analytics tools."
                }
            };

            dbContext.Features.AddRange(features);
            await dbContext.SaveChangesAsync(cancellationToken);
        }
    }
}
