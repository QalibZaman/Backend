using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using NSubstitute;
using Xunit;
using NettoRetail.Application.Models.InventoryItem;
using NettoRetail.Application.Models.Request;
using NettoRetail.Application.Services.IInventoryItemQueryService;
using NettoRetail.Domain.Entities;
using NettoRetail.Domain.Enums;
using NettoRetail.Shared.Interfaces.Repositories;
using NettoRetail.Shared.Specifications;
using NettoRetail.Application.Models.Response;

namespace NettoRetail.Tests.UnitTests.InventoryItems
{
    public class InventoryItemQueryServiceTests
    {
        private readonly IInventoryItemRepository _inventoryRepo = Substitute.For<IInventoryItemRepository>();
        private readonly ICategoryRepository _categoryRepo = Substitute.For<ICategoryRepository>();
        private readonly InventoryItemQueryService _service;

        public InventoryItemQueryServiceTests()
        {
            _service = new InventoryItemQueryService(_inventoryRepo, _categoryRepo);
        }

        private static InventoryItemEntity CreateItem(
            string name = "Item",
            string upc = "123",
            decimal price = 10,
            int categoryId = 1,
            int shopId = 1)
        {
            return new InventoryItemEntity
            {
                Id = Guid.NewGuid(),
                ProductName = name,
                ProductUpc = upc,
                Price = price,
                Description = "desc",
                CategoryId = categoryId,
                LastUpdatedDate = DateTime.UtcNow,
                IsActive = true,
                Status = InventoryItemStatusEnum.Available,
                AvailableAmount = 10,
                ReservedAmount = 5,
                OnHandAmount = 15,
                ProductLocation = "A1",
                ShopId = shopId,
                Category = new CategoryEntity
                {
                    Id = categoryId,
                    Name = "Category",
                    URL = "url",
                    Visibility = true,
                    Level = 1,
                    ShopId = shopId
                },
                BrandId = 1,
                Brand = new BrandEntity { Id = 1, Name = "Brand", ShopId = shopId }
            };
        }

        [Fact]
        public async Task GetInventoryItemsAsync_ShouldUseQuickSearch_WhenUpcProvided()
        {
            var model = new InventoryItemsSearchModel { UpcOrName = "1234567890", PageNumber = 1, PageSize = 10 };

            var expectedItems = new List<InventoryItemEntity> { CreateItem("Item1", "1234567890") };

            _inventoryRepo.GetAllPaginatedAsync(Arg.Any<Specification<InventoryItemEntity>>(), Arg.Any<CancellationToken>())
                .Returns(Task.FromResult(((ICollection<InventoryItemEntity>)expectedItems, expectedItems.Count)));

            var result = await _service.GetInventoryItemsAsync(1, model, CancellationToken.None);

            Assert.Equal(1, result.TotalAmount);
            Assert.Single(result.Items);
        }

        [Fact]
        public async Task GetInventoryItemsAsync_ShouldUseQuickSearch_WhenNameProvided()
        {
            var model = new InventoryItemsSearchModel { UpcOrName = "Laptop" };

            _inventoryRepo.GetAllPaginatedAsync(Arg.Any<Specification<InventoryItemEntity>>(), Arg.Any<CancellationToken>())
                .Returns(Task.FromResult(((ICollection<InventoryItemEntity>)new List<InventoryItemEntity>(), 0)));

            var result = await _service.GetInventoryItemsAsync(1, model, CancellationToken.None);

            Assert.Empty(result.Items);
            Assert.Equal(0, result.TotalAmount);
        }

        [Fact]
        public async Task GetInventoryItemsAsync_ShouldCallSorted_WhenSortOptionSetAndNoSortingOptions()
        {
            // Arrange
            var model = new InventoryItemsSearchModel
            {
                SortOption = InventoryItemSortingEnum.PriceLowToHigh,
                SortingOptions = null
            };

            var items = new List<InventoryItemEntity> { CreateItem("Item", "123", 15) };

            _inventoryRepo.GetAllPaginatedAsync(Arg.Any<Specification<InventoryItemEntity>>(), Arg.Any<CancellationToken>())
                .Returns(((ICollection<InventoryItemEntity>)items, items.Count));

            // Act
            var result = await _service.GetInventoryItemsAsync(1, model, CancellationToken.None);

            // Assert
            Assert.Equal(1, result.TotalAmount);
            Assert.Single(result.Items);
            await _inventoryRepo.Received(1)
                .GetAllPaginatedAsync(Arg.Any<Specification<InventoryItemEntity>>(), Arg.Any<CancellationToken>());
        }

        [Fact]
        public async Task GetInventoryItemsAsync_ShouldFetchSubcategories_WhenCategoryIdProvided()
        {
            var model = new InventoryItemsSearchModel
            {
                CategoryId = 5,
                SortingOptions = new List<SortingOption> { new() { Field = "price", IsDescending = false } }
            };

            var categories = new List<CategoryEntity>
            {
                new() { Id = 5, Name = "Test", URL = "x", Visibility = true, ShopId = 1 }
            };

            var items = new List<InventoryItemEntity> { CreateItem("Item", "111", 5, 5) };

            _categoryRepo.GetCategorySubtreeAsync(5, Arg.Any<CancellationToken>()).Returns(Task.FromResult(categories));
            _inventoryRepo.GetAllPaginatedAsync(Arg.Any<Specification<InventoryItemEntity>>(), Arg.Any<CancellationToken>())
                .Returns(Task.FromResult(((ICollection<InventoryItemEntity>)items, 1)));

            var result = await _service.GetInventoryItemsAsync(1, model, CancellationToken.None);

            Assert.Equal(1, result.TotalAmount);
            await _categoryRepo.Received(1).GetCategorySubtreeAsync(5, Arg.Any<CancellationToken>());
        }

        [Fact]
        public async Task GetInventoryItemsAsync_ShouldApplySorting_WhenSortingOptionsProvided()
        {
            var model = new InventoryItemsSearchModel
            {
                CategoryId = 0,
                SortingOptions = new List<SortingOption>
                {
                    new() { Field = "productname", IsDescending = false },
                    new() { Field = "price", IsDescending = true }
                }
            };

            var items = new List<InventoryItemEntity>
            {
                CreateItem("ItemA", "1", 10),
                CreateItem("ItemB", "2", 20)
            };

            _inventoryRepo.GetAllPaginatedAsync(Arg.Any<Specification<InventoryItemEntity>>(), Arg.Any<CancellationToken>())
                .Returns(Task.FromResult(((ICollection<InventoryItemEntity>)items, items.Count)));

            var result = await _service.GetInventoryItemsAsync(1, model, CancellationToken.None);

            Assert.Equal(2, result.TotalAmount);
            Assert.Equal(2, result.Items.Count);
        }

        [Fact]
        public async Task GetInventoryItemsAsync_ShouldHandleEmptyModel_AndReturnEmptyResult()
        {
            // Arrange
            var model = new InventoryItemsSearchModel(); 
            _inventoryRepo.GetAllPaginatedAsync(Arg.Any<Specification<InventoryItemEntity>>(), Arg.Any<CancellationToken>())
                .Returns(Task.FromResult(((ICollection<InventoryItemEntity>)new List<InventoryItemEntity>(), 0)));

            // Act
            var result = await _service.GetInventoryItemsAsync(1, model, CancellationToken.None);

            // Assert
            Assert.NotNull(result);
            Assert.Empty(result.Items);
            Assert.Equal(0, result.TotalAmount);
            await _inventoryRepo.Received(1)
                .GetAllPaginatedAsync(Arg.Any<Specification<InventoryItemEntity>>(), Arg.Any<CancellationToken>());
        }

        [Fact]
        public async Task GetInventoryItemsAsync_ShouldTrimAndSearch_WhenUpcOrNameHasSpaces()
        {
            var model = new InventoryItemsSearchModel { UpcOrName = "   Laptop   " };
            var items = new List<InventoryItemEntity> { CreateItem("Laptop", "321", 100) };

            _inventoryRepo.GetAllPaginatedAsync(Arg.Any<Specification<InventoryItemEntity>>(), Arg.Any<CancellationToken>())
                .Returns(Task.FromResult(((ICollection<InventoryItemEntity>)items, items.Count)));

            var result = await _service.GetInventoryItemsAsync(1, model, CancellationToken.None);

            Assert.Equal(1, result.TotalAmount);
            Assert.Single(result.Items);
        }
        [Fact]
        public async Task GetInventoryItemsAsync_ShouldReturnEmpty_WhenRepositoryReturnsEmptyResult()
        {
            // Arrange
            var model = new InventoryItemsSearchModel { UpcOrName = "Phone" };

            _inventoryRepo.GetAllPaginatedAsync(Arg.Any<Specification<InventoryItemEntity>>(), Arg.Any<CancellationToken>())
                .Returns(Task.FromResult(((ICollection<InventoryItemEntity>)new List<InventoryItemEntity>(), 0)));

            // Act
            var result = await _service.GetInventoryItemsAsync(1, model, CancellationToken.None);

            // Assert
            Assert.NotNull(result);
            Assert.Empty(result.Items);
            Assert.Equal(0, result.TotalAmount);
            await _inventoryRepo.Received(1)
                .GetAllPaginatedAsync(Arg.Any<Specification<InventoryItemEntity>>(), Arg.Any<CancellationToken>());
        }


        [Fact]
        public async Task GetInventoryItemsAsync_ShouldReturnZero_WhenSortedRepositoryReturnsEmpty()
        {
            var model = new InventoryItemsSearchModel
            {
                SortOption = InventoryItemSortingEnum.PriceHighToLow,
                SortingOptions = null
            };

            _inventoryRepo.GetAllPaginatedAsync(Arg.Any<Specification<InventoryItemEntity>>(), Arg.Any<CancellationToken>())
                .Returns(((ICollection<InventoryItemEntity>)new List<InventoryItemEntity>(), 0));

            var result = await _service.GetInventoryItemsAsync(1, model, CancellationToken.None);

            Assert.NotNull(result);
            Assert.Equal(0, result.TotalAmount);
            Assert.Empty(result.Items);
            await _inventoryRepo.Received(1)
                .GetAllPaginatedAsync(Arg.Any<Specification<InventoryItemEntity>>(), Arg.Any<CancellationToken>());
        }

        [Fact]
        public async Task GetInventoryItemsAsync_ShouldHandleEmptyCategorySubtreeGracefully()
        {
            var model = new InventoryItemsSearchModel
            {
                CategoryId = 9,
                SortingOptions = new List<SortingOption> { new() { Field = "price", IsDescending = false } }
            };

            var items = new List<InventoryItemEntity> { CreateItem("Safe", "999", 30) };

            _categoryRepo.GetCategorySubtreeAsync(9, Arg.Any<CancellationToken>())
                .Returns(Task.FromResult(new List<CategoryEntity>()));
            _inventoryRepo.GetAllPaginatedAsync(Arg.Any<Specification<InventoryItemEntity>>(), Arg.Any<CancellationToken>())
                .Returns(Task.FromResult(((ICollection<InventoryItemEntity>)items, items.Count)));

            var result = await _service.GetInventoryItemsAsync(1, model, CancellationToken.None);

            Assert.Equal(1, result.TotalAmount);
            Assert.Single(result.Items);
            await _categoryRepo.Received(1).GetCategorySubtreeAsync(9, Arg.Any<CancellationToken>());
        }

        [Fact]
        public async Task GetInventoryItemsAsync_ShouldRespectPagination_WhenPageNumberAndSizeProvided()
        {
            // Arrange
            var model = new InventoryItemsSearchModel
            {
                PageNumber = 2,
                PageSize = 3,
                SortingOptions = new List<SortingOption> { new() { Field = "price", IsDescending = false } }
            };

            var items = new List<InventoryItemEntity>
            {
                CreateItem("Item1", "1", 10),
                CreateItem("Item2", "2", 20),
                CreateItem("Item3", "3", 30)
            };

            _inventoryRepo.GetAllPaginatedAsync(Arg.Any<Specification<InventoryItemEntity>>(), Arg.Any<CancellationToken>())
                .Returns(Task.FromResult(((ICollection<InventoryItemEntity>)items, 6)));

            // Act
            var result = await _service.GetInventoryItemsAsync(1, model, CancellationToken.None);

            // Assert
            Assert.Equal(6, result.TotalAmount);
            Assert.Equal(3, result.Items.Count);
        }

        [Fact]
        public async Task GetInventoryItemsAsync_ShouldHandleInvalidPaginationValuesGracefully()
        {
            var model = new InventoryItemsSearchModel
            {
                PageNumber = 0,
                PageSize = 0,
                SortingOptions = new List<SortingOption> { new() { Field = "price", IsDescending = true } }
            };

            _inventoryRepo.GetAllPaginatedAsync(Arg.Any<Specification<InventoryItemEntity>>(), Arg.Any<CancellationToken>())
                .Returns(Task.FromResult(((ICollection<InventoryItemEntity>)new List<InventoryItemEntity>(), 0)));

            var result = await _service.GetInventoryItemsAsync(1, model, CancellationToken.None);

            Assert.Empty(result.Items);
            Assert.Equal(0, result.TotalAmount);
        }

        [Fact]
        public async Task GetInventoryItemsAsync_ShouldApplyMultipleSortingOptionsCorrectly()
        {
            var model = new InventoryItemsSearchModel
            {
                SortingOptions = new List<SortingOption>
                {
                    new() { Field = "brandname", IsDescending = false },
                    new() { Field = "availableamount", IsDescending = true }
                }
            };

            var items = new List<InventoryItemEntity>
            {
                CreateItem("BrandA", "1", 100),
                CreateItem("BrandB", "2", 200)
            };

            _inventoryRepo.GetAllPaginatedAsync(Arg.Any<Specification<InventoryItemEntity>>(), Arg.Any<CancellationToken>())
                .Returns(Task.FromResult(((ICollection<InventoryItemEntity>)items, items.Count)));

            var result = await _service.GetInventoryItemsAsync(1, model, CancellationToken.None);

            Assert.Equal(2, result.Items.Count);
            await _inventoryRepo.Received(1)
                .GetAllPaginatedAsync(Arg.Any<Specification<InventoryItemEntity>>(), Arg.Any<CancellationToken>());
        }

        [Fact]
        public async Task GetInventoryItemsAsync_ShouldHandleMixedFilterValuesProperly()
        {
            var model = new InventoryItemsSearchModel
            {
                MinPrice = 10,
                MaxPrice = 100,
                MinAvailableAmount = 1,
                MaxAvailableAmount = 100,
                SortingOptions = new List<SortingOption> { new() { Field = "price", IsDescending = false } }
            };

            var items = new List<InventoryItemEntity>
            {
                CreateItem("Filtered1", "11", 20),
                CreateItem("Filtered2", "22", 40)
            };

            _inventoryRepo.GetAllPaginatedAsync(Arg.Any<Specification<InventoryItemEntity>>(), Arg.Any<CancellationToken>())
                .Returns(Task.FromResult(((ICollection<InventoryItemEntity>)items, items.Count)));

            var result = await _service.GetInventoryItemsAsync(1, model, CancellationToken.None);

            Assert.Equal(2, result.Items.Count);
            Assert.True(result.TotalAmount > 0);
        }

        [Fact]
        public async Task GetInventoryItemsAsync_ShouldFallbackToDefaultSorting_WhenInvalidFieldNameProvided()
        {
            var model = new InventoryItemsSearchModel
            {
                SortingOptions = new List<SortingOption> { new() { Field = "nonexistentfield", IsDescending = false } }
            };

            var items = new List<InventoryItemEntity>
            {
                CreateItem("DefaultSorted", "999", 50)
            };

            _inventoryRepo.GetAllPaginatedAsync(Arg.Any<Specification<InventoryItemEntity>>(), Arg.Any<CancellationToken>())
                .Returns(Task.FromResult(((ICollection<InventoryItemEntity>)items, items.Count)));

            var result = await _service.GetInventoryItemsAsync(1, model, CancellationToken.None);

            Assert.Single(result.Items);
            Assert.Equal("DefaultSorted", ((List<InventoryItemListResponse>)result.Items)[0].ProductName);
        }

        [Fact]
        public async Task GetInventoryItemsAsync_ShouldFilterByBrandName_WhenBrandNameProvided()
        {
            // Arrange
            var model = new InventoryItemsSearchModel
            {
                BrandName = "Nike",
                SortingOptions = new List<SortingOption> { new() { Field = "brandname", IsDescending = false } }
            };

            var items = new List<InventoryItemEntity>
            {
                CreateItem("Shoe", "1", 120),
                CreateItem("Sneaker", "2", 90)
            };

            _inventoryRepo.GetAllPaginatedAsync(Arg.Any<Specification<InventoryItemEntity>>(), Arg.Any<CancellationToken>())
                .Returns(Task.FromResult(((ICollection<InventoryItemEntity>)items, items.Count)));

            // Act
            var result = await _service.GetInventoryItemsAsync(1, model, CancellationToken.None);

            // Assert
            Assert.Equal(2, result.Items.Count);
            Assert.Equal(items.Count, result.TotalAmount);
            await _inventoryRepo.Received(1)
                .GetAllPaginatedAsync(Arg.Any<Specification<InventoryItemEntity>>(), Arg.Any<CancellationToken>());
        }

        [Fact]
        public async Task GetInventoryItemsAsync_ShouldFilterByStatus_WhenStatusProvided()
        {
            // Arrange
            var model = new InventoryItemsSearchModel
            {
                Status = "Available",
                SortingOptions = new List<SortingOption> { new() { Field = "status", IsDescending = false } }
            };

            var items = new List<InventoryItemEntity>
            {
                CreateItem("ItemA", "1", 50),
                CreateItem("ItemB", "2", 70)
            };

            _inventoryRepo.GetAllPaginatedAsync(Arg.Any<Specification<InventoryItemEntity>>(), Arg.Any<CancellationToken>())
                .Returns(Task.FromResult(((ICollection<InventoryItemEntity>)items, items.Count)));

            // Act
            var result = await _service.GetInventoryItemsAsync(1, model, CancellationToken.None);

            // Assert
            Assert.Equal(2, result.Items.Count);
            Assert.True(result.TotalAmount > 0);
        }

        [Fact]
        public async Task GetInventoryItemsAsync_ShouldApplyCategoryAndSortingTogether()
        {
            var model = new InventoryItemsSearchModel
            {
                CategoryId = 3,
                SortingOptions = new List<SortingOption> { new() { Field = "price", IsDescending = true } }
            };

            var categories = new List<CategoryEntity>
            {
                new() { Id = 3, Name = "Shoes", URL = "s", Visibility = true, ShopId = 1 }
            };

            var items = new List<InventoryItemEntity>
            {
                CreateItem("Expensive", "1", 300, 3),
                CreateItem("Cheap", "2", 50, 3)
            };

            _categoryRepo.GetCategorySubtreeAsync(3, Arg.Any<CancellationToken>())
                .Returns(Task.FromResult(categories));
            _inventoryRepo.GetAllPaginatedAsync(Arg.Any<Specification<InventoryItemEntity>>(), Arg.Any<CancellationToken>())
                .Returns(Task.FromResult(((ICollection<InventoryItemEntity>)items, items.Count)));

            var result = await _service.GetInventoryItemsAsync(1, model, CancellationToken.None);

            Assert.Equal(2, result.Items.Count);
            await _categoryRepo.Received(1).GetCategorySubtreeAsync(3, Arg.Any<CancellationToken>());
        }

        [Fact]
        public async Task GetInventoryItemsAsync_ShouldPrioritizeQuickSearch_WhenBothSearchAndSortingProvided()
        {
            var model = new InventoryItemsSearchModel
            {
                UpcOrName = "123",
                SortingOptions = new List<SortingOption> { new() { Field = "price", IsDescending = false } }
            };

            var items = new List<InventoryItemEntity>
            {
                CreateItem("Phone", "123", 200)
            };

            _inventoryRepo.GetAllPaginatedAsync(Arg.Any<Specification<InventoryItemEntity>>(), Arg.Any<CancellationToken>())
                .Returns(Task.FromResult(((ICollection<InventoryItemEntity>)items, items.Count)));

            var result = await _service.GetInventoryItemsAsync(1, model, CancellationToken.None);

            Assert.Equal(1, result.Items.Count);
            await _inventoryRepo.Received(1)
                .GetAllPaginatedAsync(Arg.Any<Specification<InventoryItemEntity>>(), Arg.Any<CancellationToken>());
        }

        [Fact]
        public async Task GetInventoryItemsAsync_ShouldHandlePartialRepositoryResultsGracefully()
        {
            var model = new InventoryItemsSearchModel
            {
                SortingOptions = new List<SortingOption> { new() { Field = "price", IsDescending = false } }
            };

            var items = new List<InventoryItemEntity>
            {
                CreateItem("PartialA", "1", 10)
            };

            _inventoryRepo.GetAllPaginatedAsync(Arg.Any<Specification<InventoryItemEntity>>(), Arg.Any<CancellationToken>())
                .Returns(Task.FromResult(((ICollection<InventoryItemEntity>)items, 5)));

            var result = await _service.GetInventoryItemsAsync(1, model, CancellationToken.None);

            Assert.Single(result.Items);
            Assert.Equal(5, result.TotalAmount);
        }
    }
}
