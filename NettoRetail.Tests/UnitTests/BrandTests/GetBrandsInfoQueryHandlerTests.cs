using MapsterMapper;
using NettoRetail.Application.CQRS.Brands.Queries;
using NettoRetail.Application.Exceptions;
using NettoRetail.DataAccess.Specifications.Brands;
using NettoRetail.Domain.Entities;
using NettoRetail.Shared.Interfaces.Repositories;
using NettoRetail.Tests.Helpers;
using NSubstitute;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection.Metadata;
using System.Text;
using System.Threading.Tasks;
using static NettoRetail.Shared.Constants.ValidationErrorMessages;

namespace NettoRetail.Tests.UnitTests.BrandTests
{
    public class GetBrandsInfoQueryHandlerTests
    {
        private readonly IBrandRepository _brandRepository;
        private readonly IMapper _mapper;
        private readonly GetBrandInfoQueryHandler _handler;
        
        public GetBrandsInfoQueryHandlerTests()
        {
            _brandRepository = Substitute.For<IBrandRepository>();
            _mapper = MapperFactory.Create();
            _handler = new GetBrandInfoQueryHandler(_brandRepository, _mapper);
        }
        
        [Fact]
        public async Task Handle_ShouldThrowNotFound_WhenBrandDoesNotExist()
        {
            // Arrange
            var query = new GetBrandInfoQuery(999, 1);

            _brandRepository.GetAsync(Arg.Any<GetBrandsByIdSpecification>(), Arg.Any<CancellationToken>())
                .Returns((BrandEntity?)null);
            // Act & Assert
            await Assert.ThrowsAsync<NotFoundException>(() => 
                _handler.Handle(query, CancellationToken.None));
        }

        [Fact]
        public async Task Handle_ShouldThrowMyUnauthorizeException_WhenShopIdDoesNotMatch()
        {
            // Arrange
            var query = new GetBrandInfoQuery(1, 999);
            var brandEntity = new BrandEntity
            {
                Id = 1,
                Name = "Test Brand",
                Description = "Test Description",
                WebsiteUrl = "http://testbrand.com",
                LogoImageUrl = "http://testbrand.com/logo.png",
                ShopId = 1
            };
            _brandRepository.GetAsync(Arg.Any<GetBrandsByIdSpecification>(), Arg.Any<CancellationToken>())
                .Returns(brandEntity);
            // Act & Assert
            await Assert.ThrowsAsync<MyUnauthorizedException>(() => 
                _handler.Handle(query, CancellationToken.None));
        }

        [Fact]
        public async Task Handler_ShouldReturnValidResponse_WhenBrandExistsAndShopIdMatches()
        {
            // Arrange
            var query = new GetBrandInfoQuery(1, 1);
            var brandEntity = new BrandEntity
            {
                Id = 1,
                Name = "Test Brand",
                Description = "Test Description",
                WebsiteUrl = "http://testbrand.com",
                LogoImageUrl = "http://testbrand.com/logo.png",
                ShopId = 1
            };

            _brandRepository.GetAsync(Arg.Any<GetBrandsByIdSpecification>(), Arg.Any<CancellationToken>())
                .Returns(brandEntity);
            // Act
            var result = await _handler.Handle(query, CancellationToken.None);
            // Assert
            Assert.NotNull(result);
            Assert.Equal(brandEntity.Id, result.Id);
            Assert.Equal(brandEntity.Name, result.Name);
            Assert.Equal(brandEntity.Description, result.Description);
            Assert.Equal(brandEntity.WebsiteUrl, result.WebsiteUrl);
            Assert.Equal(brandEntity.LogoImageUrl, result.LogoImageUrl);
        }
    }
}
