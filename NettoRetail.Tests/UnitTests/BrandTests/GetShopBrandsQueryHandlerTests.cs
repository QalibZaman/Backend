using MapsterMapper;
using NettoRetail.Application.CQRS.Brands.Queries;
using NettoRetail.Application.Models.Request;
using NettoRetail.Domain.Entities;
using NettoRetail.Shared.Interfaces.Repositories;
using NettoRetail.Shared.Specifications;
using NettoRetail.Tests.Helpers;
using NSubstitute;

namespace NettoRetail.Tests.UnitTests.BrandTests
{
    public class GetShopBrandsQueryHandlerTests
    {
        private readonly IBrandRepository _brandRepository;
        private readonly IMapper _mapper;
        private readonly GetShopBrandsQueryHandler _handler;

        public GetShopBrandsQueryHandlerTests()
        {
            _brandRepository = Substitute.For<IBrandRepository>();
            _mapper = MapperFactory.Create();

            _handler = new GetShopBrandsQueryHandler(
                _brandRepository,
                _mapper);
        }

        [Fact]
        public async Task Handle_ShouldReturnPaginatedBrands()
        {
            // Arrange
            var query = new GetShopBrandsQuery(
                10,
                new PaginationRequest
                {
                    PageNumber = 1,
                    PageSize = 10
                });

            var brands = new List<BrandEntity>
        {
            new()
            {
                Id = 1,
                Name = "Apple",
                Description = "Apple description",
                WebsiteUrl = "https://apple.com",
                LogoImageUrl = "https://apple.com/logo.png",
                ShopId = 10
            },
            new()
            {
                Id = 2,
                Name = "Samsung",
                Description = "Samsung description",
                WebsiteUrl = "https://samsung.com",
                LogoImageUrl = "https://samsung.com/logo.png",
                ShopId = 10
            }
        };

            _brandRepository
                .GetAllPaginatedAsync(
                    Arg.Any<Specification<BrandEntity>>(),
                    Arg.Any<CancellationToken>())
                .Returns((brands, brands.Count));

            // Act
            var result = await _handler.Handle(query, CancellationToken.None);

            // Assert
            Assert.NotNull(result);
            Assert.Equal(2, result.TotalAmount);
            Assert.Equal(2, result.Items.Count);

            Assert.Equal(1, result.Items.First().Id);
            Assert.Equal("Apple", result.Items.First().Name);
            Assert.Equal("Apple description", result.Items.First().Description);
            Assert.Equal("https://apple.com", result.Items.First().WebsiteUrl);
            Assert.Equal("https://apple.com/logo.png", result.Items.First().LogoImageUrl);
        }

        [Fact]
        public async Task Handle_ShouldReturnEmptyCollection_WhenNoBrandsExist()
        {
            // Arrange
            var query = new GetShopBrandsQuery(
                10,
                new PaginationRequest
                {
                    PageNumber = 1,
                    PageSize = 10
                });

            _brandRepository
                .GetAllPaginatedAsync(
                    Arg.Any<Specification<BrandEntity>>(),
                    Arg.Any<CancellationToken>())
                .Returns((new List<BrandEntity>(), 0));

            // Act
            var result = await _handler.Handle(query, CancellationToken.None);

            // Assert
            Assert.NotNull(result);
            Assert.Empty(result.Items);
            Assert.Equal(0, result.TotalAmount);
        }
    }
}
