using MapsterMapper;
using Microsoft.EntityFrameworkCore.Storage;
using NettoRetail.Application.CQRS.Shops.Commands;
using NettoRetail.Application.Exceptions;
using NettoRetail.Application.Models.Country;
using NettoRetail.Application.Models.Response;
using NettoRetail.Application.Models.Shop;
using NettoRetail.Application.Services.ICountryValidationService;
using NettoRetail.Application.Services.IShopValidationService;
using NettoRetail.DataAccess.Specifications.Staff;
using NettoRetail.Domain.Entities;
using NettoRetail.Shared.Interfaces.Repositories;
using NettoRetail.Shared.Interfaces.Utilities;
using NSubstitute;
using NSubstitute.ExceptionExtensions;
using System;
using System.Collections.Generic;
using System.Data;
using System.Dynamic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Transactions;

namespace NettoRetail.Tests.UnitTests.ShopTests
{
    public class CreateShopCommandHandlerTests
    {
        private readonly CreateShopCommandHandler _handler;
        private readonly IShopRepository _shopRepository;
        private readonly IStaffRepository _staffRepository;
        private readonly IUnitOfWork _unitOfWork;
        private readonly ICountryDetailsValidationService _countryDetailsValidator;
        private readonly IShopValidationService _shopValidator;
        private readonly IMapper _mapper;
        private readonly IDbTransaction _transaction;

        public CreateShopCommandHandlerTests()
        {
            _shopRepository = Substitute.For<IShopRepository>();
            _staffRepository = Substitute.For<IStaffRepository>();
            _unitOfWork = Substitute.For<IUnitOfWork>();
            _countryDetailsValidator = Substitute.For<ICountryDetailsValidationService>();
            _shopValidator = Substitute.For<IShopValidationService>();
            _mapper = Substitute.For<IMapper>();
            _transaction = Substitute.For<IDbTransaction>();

            _unitOfWork.BeginTransaction().Returns(_transaction);
            _shopRepository.AddAsync(Arg.Any<ShopEntity>(), Arg.Any<CancellationToken>())
                  .Returns(CallInfo => CallInfo.Arg<ShopEntity>());

            _mapper.Map<ShopEntity>(Arg.Any<ShopModel>())
                .Returns(CallInfo =>
                {
                    var model = CallInfo.Arg<ShopModel>();
                    return new ShopEntity
                    {
                        Id = 0,
                        Name = model.Name,
                        PhoneNumber = model.PhoneNumber,
                        Email = model.Email,
                        Url = model.Url,
                        CountryId = model.CountryId,
                        IsActive = true,
                        SubscriptionPlanId = 0
                    };
                });

            _mapper.Map<ShopResponse>(Arg.Any<ShopEntity>())
                .Returns(CallInfo =>
                {
                    var entity = CallInfo.Arg<ShopEntity>();
                    return new ShopResponse
                    {
                        Id = entity.Id,
                        Name = entity.Name,
                        PhoneNumber = entity.PhoneNumber,
                        Email = entity.Email,
                        Url = entity.Url,
                        Address = entity.Address ?? string.Empty,
                        CountryId = entity.CountryId ?? 0,
                        City = entity.City ?? string.Empty,
                        State = entity.State ?? string.Empty,
                        ZipCode = entity.ZipCode ?? string.Empty,
                        Description = entity.Description ?? string.Empty,
                        Subscription = string.Empty,
                    };
                });

            _handler = new CreateShopCommandHandler(
                _shopRepository,
                _staffRepository,
                _unitOfWork,
                _countryDetailsValidator,
                _shopValidator,
                _mapper);
        }

        [Fact]
        public async Task CreateShop_ValidRequest_ReturnsShopResponse()
        {
            // Arrange
            var accountId = Guid.NewGuid();
            int shopId = 1;
            var model = new ShopModel { Name = "Test Shop", Url = "testshop", Email = "test@example.com", PhoneNumber = "1234567890" };

            var account = new StaffEntity
            {
                Id = accountId,
                ShopId = null,
                FirstName = "Test",
                LastName = "User",
                Email = "account@example.com",
                PasswordHash = "hash"
            };
            var createdShop = new ShopEntity
            {
                Id = shopId,
                Name = "Test Shop",
                Email = model.Email,
                PhoneNumber = model.PhoneNumber,
                Url = model.Url,
                IsActive = true,
                SubscriptionPlanId = 0
            };

            _staffRepository
                .GetAsync(Arg.Any<GetStaffByIdSpecification>(), Arg.Any<CancellationToken>())
                .Returns(account);

            _shopRepository
                .AddAsync(Arg.Any<ShopEntity>(), Arg.Any<CancellationToken>())
                .Returns(createdShop);

            // Act
            var result = await _handler.Handle(new CreateShopCommand(accountId, model), CancellationToken.None);

            // Assert
            Assert.NotNull(result);
            Assert.Equal(shopId, result.Id);

            await _shopRepository.Received(1).AddAsync(Arg.Any<ShopEntity>(), Arg.Any<CancellationToken>());
            await _staffRepository.Received(1).UpdateAsync(account, Arg.Any<CancellationToken>());
            _transaction.Received(1).Commit();
        }

        [Fact]
        public async Task CreateShop_AccountNotFound_ThrowsNotFoundException()
        {
            // Arrange
            var accountId = Guid.NewGuid();
            var model = new ShopModel { Name = "Test Shop", Url = "testshop", Email = "test@example.com", PhoneNumber = "1234567890" };

            _staffRepository
                .GetAsync(Arg.Any<GetStaffByIdSpecification>(), Arg.Any<CancellationToken>())
                .Returns((StaffEntity?)null);

            // Act & Assert
            await Assert.ThrowsAsync<NotFoundException>(() =>
                _handler.Handle(new CreateShopCommand(accountId, model), CancellationToken.None));

            await _shopRepository.DidNotReceive().AddAsync(Arg.Any<ShopEntity>(), Arg.Any<CancellationToken>());
            _transaction.DidNotReceive().Commit();
        }

        [Fact]
        public async Task CreateShop_AccountAlreadyHasShop_ThrowsBadRequestException()
        {
            // Arrange
            var accountId = Guid.NewGuid();
            var model = new ShopModel { Name = "Test Shop", Url = "testshop", Email = "test@example.com", PhoneNumber = "1234567890" };
            var account = new StaffEntity
            {
                Id = accountId,
                ShopId = 1,
                FirstName = "Test",
                LastName = "User",
                Email = "account@example.com",
                PasswordHash = "hash"
            };

            _staffRepository
                .GetAsync(Arg.Any<GetStaffByIdSpecification>(), Arg.Any<CancellationToken>())
                .Returns(account);

            // Act & Assert
            await Assert.ThrowsAsync<BadRequestException>(() =>
                _handler.Handle(new CreateShopCommand(accountId, model), CancellationToken.None));

            await _shopRepository.DidNotReceive().AddAsync(Arg.Any<ShopEntity>(), Arg.Any<CancellationToken>());
            _transaction.DidNotReceive().Commit();
        }

        [Fact]
        public async Task CreateShop_WithCountryId_CallsCountryValidator()
        {
            // Arrange
            var accountId = Guid.NewGuid();
            var model = new ShopModel { Name = "Test Shop", Url = "testshop", CountryId = 1, Email = "test@example.com", PhoneNumber = "1234567890" };
            var account = new StaffEntity
            {
                Id = accountId,
                ShopId = null,
                FirstName = "Test",
                LastName = "User",
                Email = "account@example.com",
                PasswordHash = "hash"
            };
            var createdShop = new ShopEntity
            {
                Id = 2,
                Name = "Test Shop",
                Email = model.Email,
                PhoneNumber = model.PhoneNumber,
                Url = model.Url,
                IsActive = true,
                SubscriptionPlanId = 0
            };

            _staffRepository
                .GetAsync(Arg.Any<GetStaffByIdSpecification>(), Arg.Any<CancellationToken>())
                .Returns(account);

            _shopRepository
                .AddAsync(Arg.Any<ShopEntity>(), Arg.Any<CancellationToken>())
                .Returns(createdShop);

            // Act
            await _handler.Handle(new CreateShopCommand(accountId, model), CancellationToken.None);

            // Assert
            await _countryDetailsValidator
                .Received(1)
                .ValidateAsync(Arg.Any<CountryValidationModel>(), Arg.Any<CancellationToken>());
        }

        [Fact]
        public async Task CreateShop_WithoutCountryId_SkipsCountryValidator()
        {
            // Arrange
            var accountId = Guid.NewGuid();
            var model = new ShopModel { Name = "Test Shop", Url = "testshop", CountryId = null, Email = "test@example.com", PhoneNumber = "1234567890" };
            var account = new StaffEntity
            {
                Id = accountId,
                ShopId = null,
                FirstName = "Test",
                LastName = "User",
                Email = "account@example.com",
                PasswordHash = "hash"
            };
            var createdShop = new ShopEntity
            {
                Id = 3,
                Name = "Test Shop",
                Email = model.Email,
                PhoneNumber = model.PhoneNumber,
                Url = model.Url,
                IsActive = true,
                SubscriptionPlanId = 0
            };

            _staffRepository
                .GetAsync(Arg.Any<GetStaffByIdSpecification>(), Arg.Any<CancellationToken>())
                .Returns(account);

            _shopRepository
                .AddAsync(Arg.Any<ShopEntity>(), Arg.Any<CancellationToken>())
                .Returns(createdShop);

            // Act
            await _handler.Handle(new CreateShopCommand(accountId, model), CancellationToken.None);

            // Assert
            await _countryDetailsValidator
                .DidNotReceive()
                .ValidateAsync(Arg.Any<CountryValidationModel>(), Arg.Any<CancellationToken>());
        }

        [Fact]
        public async Task CreateShop_RepositoryThrowsException_RollsBackTransaction()
        {
            // Arrange
            var accountId = Guid.NewGuid();
            var model = new ShopModel { Name = "Test Shop", Url = "testshop", Email = "test@example.com", PhoneNumber = "1234567890" };
            var account = new StaffEntity
            {
                Id = accountId,
                ShopId = null,
                FirstName = "Test",
                LastName = "User",
                Email = "account@example.com",
                PasswordHash = "hash"
            };

            _staffRepository
                .GetAsync(Arg.Any<GetStaffByIdSpecification>(), Arg.Any<CancellationToken>())
                .Returns(account);

            _shopRepository
                .AddAsync(Arg.Any<ShopEntity>(), Arg.Any<CancellationToken>())
                .Throws(new Exception("DB error"));

            // Act & Assert
            await Assert.ThrowsAsync<BadRequestException>(() =>
                _handler.Handle(new CreateShopCommand(accountId, model), CancellationToken.None));

            _transaction.Received(1).Rollback();
            _transaction.DidNotReceive().Commit();
        }

        [Fact]
        public async Task CreateShop_ValidRequest_AssignsShopIdToAccount()
        {
            // Arrange
            var accountId = Guid.NewGuid();
            int shopId = 4;
            var model = new ShopModel { Name = "Test Shop", Url = "testshop", Email = "test@example.com", PhoneNumber = "1234567890" };
            var account = new StaffEntity
            {
                Id = accountId,
                ShopId = null,
                FirstName = "Test",
                LastName = "User",
                Email = "account@example.com",
                PasswordHash = "hash"
            };
            var createdShop = new ShopEntity
            {
                Id = shopId,
                Name = "Test Shop",
                Email = model.Email,
                PhoneNumber = model.PhoneNumber,
                Url = model.Url,
                IsActive = true,
                SubscriptionPlanId = 0
            };

            _staffRepository
                .GetAsync(Arg.Any<GetStaffByIdSpecification>(), Arg.Any<CancellationToken>())
                .Returns(account);

            _shopRepository
                .AddAsync(Arg.Any<ShopEntity>(), Arg.Any<CancellationToken>())
                .Returns(createdShop);

            // Act
            await _handler.Handle(new CreateShopCommand(accountId, model), CancellationToken.None);

            // Assert
            Assert.Equal(shopId, account.ShopId);
            await _staffRepository.Received(1).UpdateAsync(
                 Arg.Is<StaffEntity>(a => a.ShopId == shopId),
                 Arg.Any<CancellationToken>());

        }
    }
}
