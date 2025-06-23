using MediatR;
using NettoRetail.Domain.Entities;
using NettoRetail.Application.Exceptions;
using NettoRetail.Shared.Interfaces.Repositories;
using NettoRetail.Infrastructure.Models;
using NettoRetail.Infrastructure.Interfaces;
using NettoRetail.Shared.Specifications;
using NettoRetail.DataAccess.Specifications.Account;
using NettoRetail.Shared.Constants;

namespace NettoRetail.Application.CQRS.Subscriptions.Commands;

public record UpdateSubscriptionPlanCommand(
    int PlanId,
    Guid UserId,
    string Name,
    int Price,
    string? Description,
    string? Motto,
    List<int> FeatureIds
) : IRequest;

public class UpdateSubscriptionPlanCommandHandler(
    ISubscriptionPlanRepository planRepository,
    IAccountRepository accountRepository,
    IFeatureRepository featureRepository,
    IPriceHistoryRepository priceHistoryRepository,
    IEmailSender emailSender
) : IRequestHandler<UpdateSubscriptionPlanCommand>
{
    public async Task Handle(UpdateSubscriptionPlanCommand request, CancellationToken cancellationToken)
    {
        // 1. Проверка, что пользователь — SuperAdmin
        var superAdmin = await accountRepository.GetAsync(
            new GetAccountByIdSpecification(request.UserId), cancellationToken)
            ?? throw new NotFoundException("This account was not found");

        if (superAdmin.RoleId != RoleConstants.SuperAdmin)
            throw new ForbiddenException("Only SuperAdmin can update subscription plans.");

        // 2. Валидация features (на бэке обязательна — бизнес-правило!)
        if (request.FeatureIds == null || request.FeatureIds.Count == 0)
            throw new InvalidDataException("At least one feature must be enabled for a subscription plan.");

        // 3. Получаем план с подписчиками и features
        var specification = new SpecificationBuilder<SubscriptionPlanEntity>()
            .AddCriteria(plan => plan.Id == request.PlanId)
            .AddInclude(p => p.Accounts)
            .AddInclude(p => p.Features)
            .Build();

        var plan = await planRepository.GetAsync(specification, cancellationToken)
            ?? throw new NotFoundException("Subscription plan not found.");

        // 4. Обновление Features (many-to-many)
        var newFeatures = await featureRepository.GetAllByIdsAsync(request.FeatureIds, cancellationToken);
        plan.Features = newFeatures.ToList();

        // 5. Изменение имени
        if (!string.Equals(plan.Name, request.Name, StringComparison.Ordinal))
            plan.Name = request.Name;

        // 6. Изменение описания
        if (!string.Equals(plan.Description, request.Description, StringComparison.Ordinal))
            plan.Description = request.Description;

        // 7. Изменение motto
        if (!string.Equals(plan.Moto, request.Motto, StringComparison.Ordinal))
            plan.Moto = request.Motto;

        // 8. Изменение цены с отложенным применением через PriceHistory
        // Получаем текущую активную цену (EndDateTime == null)
        var activePriceHistory = await priceHistoryRepository.GetAsync(
            new SpecificationBuilder<PriceHistoryEntity>()
                .AddCriteria(ph => ph.SubscriptionPlanId == plan.Id)
                .AddCriteria(ph => ph.EndDateTime == null)
                .Build(),
            cancellationToken);

        var currentPrice = activePriceHistory?.Price ?? plan.MonthlyPrice;

        if (currentPrice != request.Price)
        {
            var effectiveDate = CalculateEffectiveDate(DateTime.UtcNow);

            // 8.1 Закрываем текущую цену (если есть)
            if (activePriceHistory != null)
            {
                activePriceHistory.EndDateTime = effectiveDate;
                await priceHistoryRepository.UpdateAsync(activePriceHistory, cancellationToken);
            }

            // 8.2 Добавляем новую цену
            var newPriceHistory = new PriceHistoryEntity
            {
                SubscriptionPlanId = plan.Id,
                Price = request.Price,
                StartDateTime = effectiveDate,
                EndDateTime = null,
                SubscriptionPlan = plan
            };
            await priceHistoryRepository.AddAsync(newPriceHistory, cancellationToken);

            // 8.3 Email-уведомление подписчикам
            var emails = plan.Accounts?
                .Select(a => a.Email)
                .Where(email => !string.IsNullOrEmpty(email))
                .Distinct()
                .ToList();

            if (emails != null && emails.Count > 0)
            {
                var subject = "Subscription price change notification";
                var message = $"The price of plan \"{plan.Name}\" will change from {currentPrice} to {request.Price} on {effectiveDate:dd.MM.yyyy}.";
                var bulkModel = new BulkEmailModel
                {
                    EmailAddresses = emails,
                    Subject = subject,
                    Message = message
                };
                await emailSender.SendBulkAsync(bulkModel);
            }
        }

        // 9. Сохраняем план (на случай изменений имени/описания/фич)
        await planRepository.UpdateAsync(plan, cancellationToken);
    }

    private static DateTime CalculateEffectiveDate(DateTime now)
    {
        var day = now.Day;
        var nextMonth = new DateTime(now.Year, now.Month, 1).AddMonths(1);
        if (day >= 1 && day <= 15)
            return nextMonth;
        return nextMonth.AddMonths(1);
    }
}
