using FluentAssertions;
using MapsterMapper;
using NettoRetail.Application.CQRS.Categories.Queries;
using NettoRetail.Application.Models.Request;
using NettoRetail.Application.Models.Response.Category;
using NettoRetail.Application.Utilities;
using NettoRetail.Domain.Entities;
using NettoRetail.Shared.Interfaces.Repositories;
using NettoRetail.Shared.Specifications;
using NSubstitute;

namespace NettoRetail.Tests.UnitTests.CategoryTests.Get;

public class GetCategoriesQueryHandlerTest
{
    private readonly ICategoryRepository _categoryRepositor;
    private readonly IMapper _mapper;
    private readonly GetCategoriesQueryHandler _handler;

    public GetCategoriesQueryHandlerTest()
    {
        _categoryRepositor = Substitute.For<ICategoryRepository>();
        _mapper = Substitute.For<IMapper>();
        _handler = new GetCategoriesQueryHandler(_categoryRepositor, _mapper);
    }

    [Fact]
    public async Task Handle_ShouldReturnAllCategoriesByShopId()
    {
        int shopId = 1;
        var model = new PaginationRequest
        {
            PageNumber = 1,
            PageSize = 10,
        };
        var query = new GetCategoriesQuery(model, shopId);
        var categoryEntities = new List<CategoryEntity>
        {
            new()
            {
                Id = 1,
                ShopId = shopId,
                Name = "Test",
                URL = "https://www.test.com",
                Level = 1,
                Visibility = true
            },
            new()
            {
                Id = 2,
                ShopId = shopId,
                Name = "Test 2",
                URL = "https://www.test2.com",
                Level = 1,
                Visibility = true
            },
            new()
            {
                Id = 3,
                ShopId = shopId,
                Name = "Test 3",
                URL = "https://www.test3.com",
                Level = 1,
                Visibility = true
            }
        };

        var expectedResponse = new List<CategoryResponse>
        {
            new()
            {
                Id = categoryEntities[0].Id,
                ShopId = categoryEntities[0].ShopId,
                Name = categoryEntities[0].Name,
                URL = categoryEntities[0].URL,
                Level = categoryEntities[0].Level,
                Visibility = categoryEntities[0].Visibility,
            },
            new()
            {
                Id = categoryEntities[1].Id,
                ShopId = categoryEntities[1].ShopId,
                Name = categoryEntities[1].Name,
                URL = categoryEntities[1].URL,
                Level = categoryEntities[1].Level,
                Visibility = categoryEntities[1].Visibility,
            },
            new()
            {
                Id = categoryEntities[2].Id,
                ShopId = categoryEntities[2].ShopId,
                Name = categoryEntities[2].Name,
                URL = categoryEntities[2].URL,
                Level = categoryEntities[2].Level,
                Visibility = categoryEntities[2].Visibility,
            },

        };
        _categoryRepositor.GetAllPaginatedAsync(Arg.Any<Specification<CategoryEntity>>(), Arg.Any<CancellationToken>())
            .Returns((categoryEntites: categoryEntities, categoryEntities.Count));
        _mapper
            .Map<List<CategoryResponse>>(categoryEntities)
            .Returns(expectedResponse);
        var result = await _handler.Handle(query, CancellationToken.None);
        result.TotalAmount.Should().Be(3);
        result.Categories.Should().BeEquivalentTo(expectedResponse);
    }

    [Fact]
    public async Task Handle_ShouldReturnEmptyListOfCategories_WhenCategoriesNotExist()
    {
        int shopId = 1;
        var model = new PaginationRequest
        {
            PageNumber = 1,
            PageSize = 10,
        };
        var query = new GetCategoriesQuery(model, shopId);
        
        var categoryEntities = new List<CategoryEntity>();
        var  expectedResponse = new List<CategoryResponse>();
        _categoryRepositor.GetAllPaginatedAsync(Arg.Any<Specification<CategoryEntity>>(), Arg.Any<CancellationToken>())
            .Returns((categoryEntities, categoryEntities.Count));
        _mapper
            .Map<List<CategoryResponse>>(categoryEntities)
            .Returns(expectedResponse);
        var result = await _handler.Handle(query, CancellationToken.None);
        result.TotalAmount.Should().Be(0);
        result.Categories.Should().BeEmpty();

    }
}