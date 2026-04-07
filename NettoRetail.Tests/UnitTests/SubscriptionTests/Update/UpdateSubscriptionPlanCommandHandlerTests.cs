using FluentAssertions;
using NettoRetail.Application.CQRS.Subscriptions.Commands;
using NettoRetail.Application.Exceptions;
using NettoRetail.DataAccess.Specifications.Staff;
using NettoRetail.DataAccess.Specifications.SubscriptionPlan;
using NettoRetail.Domain.Entities;
using NettoRetail.Shared.Constants;
using NettoRetail.Shared.Interfaces.Repositories;
using NettoRetail.Shared.Interfaces.Utilities;
using NSubstitute;
using NSubstitute.ReceivedExtensions;
using Stripe.Entitlements;
using System.Data;

namespace NettoRetail.Tests.UnitTests.SubscriptionTests.Update;

public class UpdateSubscriptionPlanCommandHandlerTests
{
    private readonly UpdateSubscriptionPlanCommandHandler _handler;
    private readonly ISubscriptionPlanRepository _subscriptionPlanRepository;
    private readonly IStaffRepository _staffRepository;
    private readonly IFeatureRepository _featureRepository;
    private readonly IPriceHistoryService _priceHistoryService;
    private readonly ISubscriptionNotificationService _subscriptionNotificationService;
    private readonly IUnitOfWork _unitOfWork;
    private readonly IDbTransaction _transaction;
    public UpdateSubscriptionPlanCommandHandlerTests()
    {
        _subscriptionPlanRepository = Substitute.For<ISubscriptionPlanRepository>();
        _featureRepository = Substitute.For<IFeatureRepository>();
        _staffRepository = Substitute.For<IStaffRepository>();
        _priceHistoryService = Substitute.For<IPriceHistoryService>();
        _subscriptionNotificationService = Substitute.For<ISubscriptionNotificationService>();
        _unitOfWork = Substitute.For<IUnitOfWork>();

        _transaction = Substitute.For<IDbTransaction>();
        _unitOfWork.BeginTransaction().Returns(_transaction);

        _handler = new UpdateSubscriptionPlanCommandHandler(
            _subscriptionPlanRepository,
            _staffRepository,
            _featureRepository,
            _priceHistoryService,
            _subscriptionNotificationService,
            _unitOfWork);

    }

    [Fact]
    public async Task Handle_ShouldThrowNotFoundException_WhenUserDoesNotExist()
    {
        //Arrange
        var command = CreateCommand();

        _staffRepository
            .GetAsync(Arg.Any<GetStaffByIdSpecification>(), Arg.Any<CancellationToken>())
            .Returns((StaffEntity?)null);

        //Act
        var act = async () => await _handler.Handle(command, CancellationToken.None);

        //Assert
        await act.Should().ThrowAsync<NotFoundException>()
            .WithMessage(string.Format(HandlerConstants.Account.AccountNotFoundMessage, command.UserId));
    }

    [Fact]
    public async Task Handle_ShouldThrowNotFoundException_WhenPlanDoesNotExist()
    {
        //Arrange
        var command = CreateCommand();
        var staff = CreateStaffEntity();
        _staffRepository
            .GetAsync(Arg.Any<GetStaffByIdSpecification>(), Arg.Any<CancellationToken>())
            .Returns(staff);

        _subscriptionPlanRepository
            .GetAsync(Arg.Any<GetSubscriptionPlanByIdSpecification>(), Arg.Any<CancellationToken>())
            .Returns((SubscriptionPlanEntity?)null);

        //Act
        var act = async () => await _handler.Handle(command, CancellationToken.None);

        //Assert
        await act.Should().ThrowAsync<NotFoundException>()
            .WithMessage(string.Format(HandlerConstants.Subscription.SubscriptionPlanNotFoundMessage, command.PlanId));
    }

    [Fact]
    public async Task Handle_ShouldUpdatePlanDetails_WithoutPlanChange_WhenPriceIsSame()
    {
        //Arrange
        var command = CreateCommand();
        var existingPlan = new SubscriptionPlanEntity
        {
            Id = command.PlanId,
            Name = "Old Name",
            Description = "Old Description",
            Moto = "Old Motto",
            MonthlyPrice = command.SubscriptionPlanRequest.Price,
            SubscriptionId = Guid.NewGuid().ToString(),
        };

        var newFeatures = new List<FeatureEntity>
        {
            new FeatureEntity { Id = 1, Name = "Feature 1" },
            new FeatureEntity { Id = 2, Name = "Feature 2" },
            new FeatureEntity { Id = 3, Name = "Feature 3" },
        };

        GenerateValidPath(command, existingPlan, newFeatures, command.SubscriptionPlanRequest.Price);

        //Act
        await _handler.Handle(command, CancellationToken.None);

        //Assert
        existingPlan.Name.Should().Be(command.SubscriptionPlanRequest.Name);
        existingPlan.Description.Should().Be(command.SubscriptionPlanRequest.Description);
        existingPlan.Moto.Should().Be(command.SubscriptionPlanRequest.Motto);
        existingPlan.Features.Should().BeEquivalentTo(newFeatures);

        await _subscriptionPlanRepository
            .Received(1)
            .UpdateAsync(existingPlan, Arg.Any<CancellationToken>());

        _unitOfWork.Received(1).BeginTransaction();
        _transaction.Received(1).Commit();

        await _priceHistoryService
            .DidNotReceive()
            .CalculateEffectiveDateAsync(Arg.Any<DateTime>());
        await _priceHistoryService
            .DidNotReceive()
            .UpdatePlanPriceAsync(Arg.Any<int>(), Arg.Any<int>(), Arg.Any<DateTime>(), Arg.Any<CancellationToken>());

        await _subscriptionNotificationService
            .DidNotReceive()
            .NotifyPriceChangedAsync(Arg.Any<SubscriptionPlanEntity>(), Arg.Any<int>(), Arg.Any<int>(),Arg.Any<DateTime>(), Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task Handle_ShouldUpdatePlanDetails_AndTriggerPriceChange_WhenPriceIsDifferent()
    {
        //Arrange
        var command = CreateCommand(price: 20);
        var existingPlan = new SubscriptionPlanEntity
        {
            Id = command.PlanId,
            Name = "Old Name",
            Description = "Old Description",
            Moto = "Old Motto",
            MonthlyPrice = 10,
            SubscriptionId = Guid.NewGuid().ToString(),
        };
        var newFeatures = new List<FeatureEntity>
        {
            new FeatureEntity { Id = 1, Name = "Feature 1" },
            new FeatureEntity { Id = 2, Name = "Feature 2" },
            new FeatureEntity { Id = 3, Name = "Feature 3" },
        };
        GenerateValidPath(command, existingPlan, newFeatures, existingPlan.MonthlyPrice);

        //Act
        await _handler.Handle(command, CancellationToken.None);

        //Assert
        existingPlan.Name.Should().Be(command.SubscriptionPlanRequest.Name);
        existingPlan.Description.Should().Be(command.SubscriptionPlanRequest.Description);
        existingPlan.Moto.Should().Be(command.SubscriptionPlanRequest.Motto);
        existingPlan.Features.Should().BeEquivalentTo(newFeatures);

        await _subscriptionPlanRepository
            .Received(1)
            .UpdateAsync(existingPlan, Arg.Any<CancellationToken>());
        _unitOfWork.Received(1).BeginTransaction();
        _transaction.Received(1).Commit();

        await _priceHistoryService
            .Received(1)
            .CalculateEffectiveDateAsync(Arg.Any<DateTime>());
        await _priceHistoryService
            .Received(1)
            .UpdatePlanPriceAsync(existingPlan.Id, command.SubscriptionPlanRequest.Price, Arg.Any<DateTime>(), Arg.Any<CancellationToken>());
        await _subscriptionNotificationService
            .Received(1)
            .NotifyPriceChangedAsync(existingPlan, 10, command.SubscriptionPlanRequest.Price, Arg.Any<DateTime>(), Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task Handle_ShouldNotReassignStrings_WhenStringsAreIdentical()
    {
        var command = CreateCommand(name: "Same Name", desc: "Same Description", motto: "Same Motto");
        var existingPlan = new SubscriptionPlanEntity
        {
            Id = command.PlanId,
            Name = "Same Name",
            Description = "Same Description",
            Moto = "Same Motto",
            MonthlyPrice = command.SubscriptionPlanRequest.Price,
            SubscriptionId = Guid.NewGuid().ToString(),
        };

        GenerateValidPath(command, existingPlan, new List<FeatureEntity>(), command.SubscriptionPlanRequest.Price);

        //Act
        await _handler.Handle(command, CancellationToken.None);

        //Assert
        await _subscriptionPlanRepository
            .Received(1)
            .UpdateAsync(existingPlan, Arg.Any<CancellationToken>());

    }

    private void GenerateValidPath(UpdateSubscriptionPlanCommand command, SubscriptionPlanEntity planToReturn, List<FeatureEntity> featuresToReturn, int currentPrice)
    {
        var staffEntity = new StaffEntity
        {
            FirstName = "Test",
            Email = "test@gmail.com",
            LastName = "Testtest",
            PasswordHash = "hashedpassword",
        };

        _staffRepository.GetAsync(Arg.Any<GetStaffByIdSpecification>(), Arg.Any<CancellationToken>())
            .Returns(staffEntity);
        _subscriptionPlanRepository.GetAsync(Arg.Any<GetSubscriptionPlanByIdSpecification>(), Arg.Any<CancellationToken>())
            .Returns(planToReturn);
        _featureRepository.GetAllByIdsAsync(Arg.Any<IEnumerable<int>>(), Arg.Any<CancellationToken>())
            .Returns(featuresToReturn);
        _priceHistoryService.GetCurrentPriceAsync(Arg.Any<int>(), Arg.Any<CancellationToken>())
            .Returns(currentPrice);
        _unitOfWork.BeginTransaction().Returns(_transaction);
    }

    private UpdateSubscriptionPlanCommand CreateCommand(int price = 10, string name = "Name", string desc = "Description", string motto = "Motto")
    {
        var request = new UpdateSubscriptionPlanRequest
        {
            Name = name,
            Description = desc,
            Motto = motto,
            Price = price,
            FeatureIds = new List<int> { 1, 2, 3 }
        };
        return new UpdateSubscriptionPlanCommand(1, Guid.NewGuid(), request);
    }
    private StaffEntity CreateStaffEntity()
    {
        var staffEntity = new StaffEntity
        {
            FirstName = "Test",
            Email = "test@gmail.com",
            LastName = "Testtest",
            PasswordHash = "hashedpassword",
        };

        return staffEntity;
    }
}