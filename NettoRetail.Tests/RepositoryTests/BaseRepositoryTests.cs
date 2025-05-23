using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;
using NettoRetail.DataAccess.Context;
using NettoRetail.DataAccess.Options;
using NettoRetail.DataAccess.Repositories;
using NettoRetail.DataAccess.Specifications.Country;
using NettoRetail.DataAccess.Specifications.Shop;
using NettoRetail.Domain.Entities;
using NettoRetail.Shared.Interfaces.Repositories;
using NSubstitute;

namespace NettoRetail.Tests.RepositoryTests;

public class BaseRepositoryTests : IDisposable
{
    private readonly AppDbContext _dbContext;
    private readonly BaseRepository<ShopEntity> _repository;
    private readonly CountryRepository _countryRepository;

    public BaseRepositoryTests()
    {
        var mockConnectionStrings = Substitute.For<IOptions<ConnectionStrings>>();
        mockConnectionStrings.Value.Returns(new ConnectionStrings
        {
            DefaultConnection = "Server=localhost;Database=nrt_test2;Port=5432;User Id=postgres;Password=admin;",
            AzureConnection = "FakeConnectionString"
        });

        var options = new DbContextOptionsBuilder<AppDbContext>()
            .UseNpgsql()
            .Options;

        _dbContext = new AppDbContext(options, mockConnectionStrings);
        InitializeDatabase();

        _countryRepository = new CountryRepository(_dbContext);
        _repository = new ShopRepository(_dbContext);
    }

    [Fact]
    public async Task AddAsync_ShouldAddEntity()
    {
        // Arrange
        var countrySpecification = new GetCountryByIdSpecification(1);
        var country = await _countryRepository.GetAsync(countrySpecification, CancellationToken.None);

        var shop = new ShopEntity
        {
            Id = 1,
            Name = "Test Shop",
            Email = "test@shop.com",
            PhoneNumber = "123 456789",
            Url = "testshop.com",
            Address = "123 Shop St",
            City = "Test City",
            State = "TS",
            ZipCode = "1234",
            Description = "Test shop description",
            CountryId = 1,
            Country = country!,
            IsActive = true,
            StripeCustomerId = "cus_test_1",
            StripeSubscriptionId = "sub_test_1"
        };

        // Act
        var addedShop = await _repository.AddAsync(shop, CancellationToken.None);

        // Assert
        var shopInDb = await _dbContext.Set<ShopEntity>().FirstOrDefaultAsync(s => s.Id == 1);
        Assert.NotNull(shopInDb);
        Assert.Equal("Test Shop", shopInDb?.Name);
    }

    [Fact]
    public async Task DeleteAsync_ShouldDeleteEntity()
    {
        // Arrange
        var countrySpecification = new GetCountryByIdSpecification(1);
        var country = await _countryRepository.GetAsync(countrySpecification, CancellationToken.None);

        var shop = new ShopEntity
        {
            Id = 1,
            Name = "Shop to Delete",
            Email = "delete@shop.com",
            PhoneNumber = "123 456789",
            Url = "http://deleteshop.com",
            Address = "123 Delete St",
            City = "Delete City",
            State = "DS",
            ZipCode = "54321",
            Description = "Shop to be deleted",
            CountryId = 1,
            Country = country!,
            IsActive = true,
            StripeCustomerId = "cus_test_1",
            StripeSubscriptionId = "sub_test_1"
        };

        await _repository.AddAsync(shop, CancellationToken.None);

        // Act
        await _repository.DeleteAsync(shop, CancellationToken.None);

        // Assert
        var deletedShop = await _dbContext.Set<ShopEntity>().FindAsync(1);
        Assert.Null(deletedShop);
    }

    [Fact]
    public async Task GetAsync_ShouldReturnEntity()
    {
        // Arrange
        var countrySpecification = new GetCountryByIdSpecification(1);
        var country = await _countryRepository.GetAsync(countrySpecification, CancellationToken.None);

        var shop = new ShopEntity
        {
            Id = 1,
            Name = "Test Shop",
            Email = "test@shop.com",
            PhoneNumber = "123456789",
            Url = "http://testshop.com",
            Address = "123 Shop St",
            City = "Test City",
            State = "TS",
            ZipCode = "12345",
            Description = "Test shop description",
            CountryId = 1,
            Country = country!,
            IsActive = true,
            StripeCustomerId = "cus_test_1",
            StripeSubscriptionId = "sub_test_1"
        };
        await _repository.AddAsync(shop, CancellationToken.None);

        var specification = new GetShopByIdSpecification(1);

        // Act
        var retrievedShop = await _repository.GetAsync(specification, CancellationToken.None);

        // Assert
        Assert.NotNull(retrievedShop);
        Assert.Equal("Test Shop", retrievedShop?.Name);
    }

    [Fact]
    public async Task GetAmountAsync_ShouldReturnCorrectCount()
    {
        // Arrange
        var countrySpecification = new GetCountryByIdSpecification(1);
        var country = await _countryRepository.GetAsync(countrySpecification, CancellationToken.None);

        var countrySpecification2 = new GetCountryByIdSpecification(2);
        var country2 = await _countryRepository.GetAsync(countrySpecification2, CancellationToken.None);

        var shop1 = new ShopEntity
        {
            Id = 1,
            Name = "Shop 1",
            Email = "shop1@shop.com",
            PhoneNumber = "123456789",
            Url = "http://shop1.com",
            Address = "123 Shop 1 St",
            City = "Shop 1 City",
            State = "SS1",
            ZipCode = "11111",
            Description = "Shop 1 description",
            CountryId = 1,
            Country = country!,
            IsActive = true,
            StripeCustomerId = "cus_test_1",
            StripeSubscriptionId = "sub_test_1"
        };

        var shop2 = new ShopEntity
        {
            Id = 2,
            Name = "Shop 2",
            Email = "shop2@shop.com",
            PhoneNumber = "987654321",
            Url = "http://shop2.com",
            Address = "123 Shop 2 St",
            City = "Shop 2 City",
            State = "SS2",
            ZipCode = "22222",
            Description = "Shop 2 description",
            CountryId = 2,
            Country = country2!,
            IsActive = true,
             StripeCustomerId = "cus_test_1",
            StripeSubscriptionId = "sub_test_1"
        };

        await _repository.AddAsync(shop1, CancellationToken.None);
        await _repository.AddAsync(shop2, CancellationToken.None);

        // Act
        var amount = await _repository.GetAmountAsync(CancellationToken.None);

        // Assert
        Assert.Equal(2, amount);
    }

    [Fact]
    public async Task DeleteAsync_WithSpecification_ShouldDeleteEntity()
    {
        // Arrange
        var countrySpecification = new GetCountryByIdSpecification(1);
        var country = await _countryRepository.GetAsync(countrySpecification, CancellationToken.None);

        var shop = new ShopEntity
        {
            Id = 1,
            Name = "Shop to Delete with Spec",
            Email = "specdelete@shop.com",
            PhoneNumber = "123456789",
            Url = "http://specdeleteshop.com",
            Address = "123 Spec Delete St",
            City = "Spec Delete City",
            State = "SDS",
            ZipCode = "112233",
            Description = "Spec delete shop description",
            CountryId = 1,
            Country = country!,
            IsActive = true,
            StripeCustomerId = "cus_test_1",
            StripeSubscriptionId = "sub_test_1"
        };

        await _repository.AddAsync(shop, CancellationToken.None);

        var specification = new GetShopByIdSpecification(1);

        // Act
        await _repository.DeleteAsync(specification, CancellationToken.None);

        // Assert
        var deletedShop = await _dbContext.Set<ShopEntity>().FindAsync(1);
        Assert.Null(deletedShop);
    }

    private void InitializeDatabase()
    {
        _dbContext.Database.EnsureCreated();
    }

    public void Dispose()
    {
        _dbContext.Database.EnsureDeleted();
    }
}
