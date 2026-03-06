using MediatR;
using Microsoft.AspNetCore.Mvc;
using NettoRetail.API.Controllers.V1;
using NettoRetail.API.Models.Response;
using NettoRetail.Application.CQRS.OfferSections.Query;
using NettoRetail.Application.Models.Request.Offer;
using NettoRetail.Application.Models.Response;
using NettoRetail.Application.Models.Response.Brand;
using NettoRetail.Application.Models.Response.Category;
using NSubstitute;
using NSubstitute.ExceptionExtensions;

namespace NettoRetail.Tests.UnitTests.OfferControllerTests;

public class GetSearchSuggestionsTests
{
    private readonly IMediator _mediator;
    private readonly OfferController _controller;

    public GetSearchSuggestionsTests()
    {
        _mediator = Substitute.For<IMediator>();
        _controller = new OfferController(_mediator);
    }

    [Fact]
    public async Task GetSearchSuggestions_ShouldReturnOk_WithSuggestions()
    {
        // Arrange
        var request = new SearchSuggestionsRequest { ShopId = 1, Query = "Laptop" };
        var expectedResponse = new SearchSuggestionsResponse
        {
            Query = "Laptop",
            Brands = new PaginatedResponse<BrandResponse>(),
            Categories = new PaginatedResponse<CategoryResponse>(),
            Products = new PaginatedResponse<InventoryItemListResponse>()
        };

        _mediator.Send(Arg.Any<GetSearchSuggestionsQuery>(), Arg.Any<CancellationToken>())
            .Returns(expectedResponse);

        // Act
        var result = await _controller.GetSearchSuggestions(request);

        // Assert
        var ok = Assert.IsType<OkObjectResult>(result);
        var api = Assert.IsType<ApiResponse<SearchSuggestionsResponse>>(ok.Value);
        Assert.Equal("Laptop", api.Data.Query);
    }

    [Fact]
    public async Task GetSearchSuggestions_ShouldReturnEmptyResponse_WhenQueryIsEmpty()
    {
        // Arrange
        var request = new SearchSuggestionsRequest { ShopId = 1, Query = "" };

        _mediator.Send(Arg.Any<GetSearchSuggestionsQuery>(), Arg.Any<CancellationToken>())
            .Returns(new SearchSuggestionsResponse
            {
                Query = "",
                Brands = new PaginatedResponse<BrandResponse>(),
                Categories = new PaginatedResponse<CategoryResponse>(),
                Products = new PaginatedResponse<InventoryItemListResponse>()
            });

        // Act
        var result = await _controller.GetSearchSuggestions(request);

        // Assert
        var ok = Assert.IsType<OkObjectResult>(result);
        var api = Assert.IsType<ApiResponse<SearchSuggestionsResponse>>(ok.Value);

        Assert.Equal("", api.Data.Query);
        Assert.Empty(api.Data.Brands.Items);
        Assert.Empty(api.Data.Categories.Items);
        Assert.Empty(api.Data.Products.Items);

    }

    [Fact]
    public async Task GetSearchSuggestions_ShouldUseDefaultPagination_WhenPageModelIsNull()
    {
        // Arrange
        var request = new SearchSuggestionsRequest
        {
            ShopId = 1,
            Query = "Laptop",
            PageModel = null
        };

        var expectedResponse = new SearchSuggestionsResponse { Query = "Laptop" };

        _mediator
            .Send(Arg.Any<GetSearchSuggestionsQuery>(), Arg.Any<CancellationToken>())
            .Returns(expectedResponse);

        // Act
        var result = await _controller.GetSearchSuggestions(request);

        // Assert
        var ok = Assert.IsType<OkObjectResult>(result);
        var api = Assert.IsType<ApiResponse<SearchSuggestionsResponse>>(ok.Value);

        Assert.Equal("Laptop", api.Data.Query);
    }

    [Fact]
    public async Task GetSearchSuggestions_ShouldReturnPartialResults_WhenSomeListsAreEmpty()
    {
        // Arrange
        var request = new SearchSuggestionsRequest { ShopId = 1, Query = "Laptop" };

        var expectedResponse = new SearchSuggestionsResponse
        {
            Query = "Laptop",
            Brands = new PaginatedResponse<BrandResponse>
            {
                Items = { new BrandResponse { Id = 1, Name = "Brand1", Description = "Desc", WebsiteUrl = "url" } }
            },
            Categories = new PaginatedResponse<CategoryResponse>(),
            Products = new PaginatedResponse<InventoryItemListResponse>
            {
                Items =
                {
                    new InventoryItemListResponse
                    {
                        Id = Guid.NewGuid(),
                        ProductName = "Laptop",
                        ProductCategory = "Electronics",
                        ProductLocation = "Shelf1",
                        ProductUpc = "12345",
                        Price = 1000,
                        Status = "Available",
                        BrandName = "Brand1",
                        LastUpdatedDate = DateTime.UtcNow
                    }
                }
            }
        };

        _mediator.Send(Arg.Any<GetSearchSuggestionsQuery>(), Arg.Any<CancellationToken>())
            .Returns(expectedResponse);

        // Act
        var result = await _controller.GetSearchSuggestions(request);

        // Assert
        var ok = Assert.IsType<OkObjectResult>(result);
        var api = Assert.IsType<ApiResponse<SearchSuggestionsResponse>>(ok.Value);

        Assert.Single(api.Data.Brands.Items);
        Assert.Empty(api.Data.Categories.Items);
        Assert.Single(api.Data.Products.Items);
    }

    [Fact]
    public async Task GetSearchSuggestions_ShouldThrowException_WhenMediatorFails()
    {
        // Arrange
        var request = new SearchSuggestionsRequest { ShopId = 1, Query = "Laptop" };

        _mediator
            .Send(Arg.Any<GetSearchSuggestionsQuery>(), Arg.Any<CancellationToken>())
            .Throws(new Exception("Something went wrong"));

        // Act & Assert
        var ex = await Assert.ThrowsAsync<Exception>(() => _controller.GetSearchSuggestions(request));
        Assert.Equal("Something went wrong", ex.Message);
    }
}
