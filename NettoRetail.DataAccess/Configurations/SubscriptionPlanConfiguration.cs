using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Microsoft.Extensions.Options;
using NettoRetail.Domain.Entities;
using NettoRetail.Shared.Options;

namespace NettoRetail.DataAccess.Configurations
{
    public class SubscriptionPlanConfiguration(IOptions<SubscritptionOptions> options) : IEntityTypeConfiguration<SubscriptionPlanEntity>
    {
        private readonly SubscritptionOptions _options = options.Value;
        public void Configure(EntityTypeBuilder<SubscriptionPlanEntity> builder)
        {
            builder.HasKey(model => model.Id);

            builder.Property(model => model.Moto)
                .HasMaxLength(70);

            builder.Property(model => model.Description)
                .HasMaxLength(500);

            builder.HasMany(model => model.Accounts)
                .WithOne(model => model.SubscriptionPlan)
                .HasForeignKey(model => model.SubscriptionPlanId);

            builder.HasData(
                new SubscriptionPlanEntity{
                    Id = 1,
                    Description = "#Description#Description#Description",
                    MonthlyPrice = 0,
                    Moto = "Free plan to test the application",
                    Type = Domain.Enums.SubscriptionTypeEnum.Free,
                    SubscriptionId = _options.FreePlanId
                },
                new SubscriptionPlanEntity
                {
                    Id = 2,
                    Description = "#Description#Description#Description",
                    MonthlyPrice = 20,
                    Moto = "Free plan to test the application",
                    Type = Domain.Enums.SubscriptionTypeEnum.Super,
                    SubscriptionId = _options.SuperPlanId
                },
                new SubscriptionPlanEntity
                {
                    Id = 3,
                    Description = "#Description#Description#Description",
                    MonthlyPrice = 30,
                    Moto = "Free plan to test the application",
                    Type = Domain.Enums.SubscriptionTypeEnum.Business,
                    SubscriptionId = _options.BussinessPlanId
                }
                );
        }
    }
}
