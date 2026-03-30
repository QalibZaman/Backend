using FluentValidation;
using NettoRetail.Application.CQRS.Subscription.Commands;
using NettoRetail.Application.Exceptions;
using NettoRetail.Application.Models.Request.Subscription;
using NettoRetail.DataAccess.Specifications.Account;
using NettoRetail.DataAccess.Specifications.Payment;
using NettoRetail.DataAccess.Specifications.Shop;
using NettoRetail.DataAccess.Specifications.SubscriptionPlan;
using NettoRetail.Shared.Interfaces.Repositories;

namespace NettoRetail.API.Infrastructure.Validators;

public class ChangeSubscriptionCommandValidator : AbstractValidator<ChangeSubscriptionCommand>
{
    private readonly IPaymentRepository _paymentRepository;
    private readonly ISubscriptionPlanRepository _subscriptionPlanRepository;
    private readonly IStaffRepository _staffRepository;
    private readonly IShopRepository _shopRepository;
    private readonly IValidator<ChangeSubscriptionRequest> _validator;
    public ChangeSubscriptionCommandValidator(
        ISubscriptionPlanRepository subscriptionPlanRepository,
        IStaffRepository staffRepository,
        IShopRepository shopRepository,
        IPaymentRepository paymentRepository,
        IValidator<ChangeSubscriptionRequest> validator)
    {
        _subscriptionPlanRepository = subscriptionPlanRepository;
        _staffRepository = staffRepository;
        _shopRepository = shopRepository;
        _paymentRepository = paymentRepository;
        _validator = validator;
        ConfigureRules();
    }

    private void ConfigureRules()
    {
        ClassLevelCascadeMode = CascadeMode.Stop;
        RuleLevelCascadeMode = CascadeMode.Stop;

        RuleFor(x => x.ChangeSubscriptionRequest)
            .SetValidator(_validator);

        RuleFor(x => x)
            .CustomAsync(ValidateSubscriptionPlanChangeAsync);

        RuleFor(x => x)
            .CustomAsync(ValidateShopAdminAccessAsync)
            .CustomAsync(ValidatePaymentMethodIdFromDatabaseAsync);
    }

    private async Task ValidateSubscriptionPlanChangeAsync(
        ChangeSubscriptionCommand command,
        ValidationContext<ChangeSubscriptionCommand> context,
        CancellationToken cancellationToken)
    {
        var plan = await _subscriptionPlanRepository
            .GetAsync(new GetSubscriptionPlanByEnumSpecification(command.ChangeSubscriptionRequest.NewPlan), cancellationToken)
                ?? throw new InvalidOperationException("Subscription plan not found");

        var shop = await _shopRepository.GetAsync(new GetShopByIdSpecification(command.ShopId), cancellationToken)
            ?? throw new NotFoundException($"Shop with ID {command.ShopId} not found.");

        if (shop.SubscriptionPlanId == plan.Id) throw new ConflictException("Shop already has this subscription plan.");
    }

    private async Task ValidateShopAdminAccessAsync(
        ChangeSubscriptionCommand command,
        ValidationContext<ChangeSubscriptionCommand> context,
        CancellationToken cancellationToken)
    {
        var admin = await _staffRepository.GetAsync(new GetStaffByIdSpecification(command.UserId), cancellationToken)
            ?? throw new NotFoundException("Shop admin with such Id dont exists");

        if (admin!.ShopId != command.ShopId) throw new ForbiddenException("You cannot change the subsciption plan of a shop you do not belong to");
    }

    private async Task ValidatePaymentMethodIdFromDatabaseAsync(
        ChangeSubscriptionCommand command,
        ValidationContext<ChangeSubscriptionCommand> context,
        CancellationToken cancellationToken)
    {
        var paymentMethodExists = await _paymentRepository.Exists(
            new GetPaymentMethodByIdSpecification(command.UserId, command.ChangeSubscriptionRequest.PaymentMethodId), cancellationToken);

        if (!paymentMethodExists)
        {
            throw new NotFoundException($"Payment Method '{command.ChangeSubscriptionRequest.PaymentMethodId}' not found for the current user.");
        }
    }
}
