using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;
using NettoRetail.DataAccess.Context;
using NettoRetail.DataAccess.Options;
using NettoRetail.DataAccess.Repositories;
using NettoRetail.Domain.Entities;
using NettoRetail.Shared.Options;
using NSubstitute;

namespace NettoRetail.Tests.RepositoryTests;

public class CategoryRepositoryTests
{
    private readonly CategoryRepository _categoryRepository;
    private readonly AppDbContext _context;

    public CategoryRepositoryTests()
    {
        var mockConnectionStrings = Substitute.For<IOptions<ConnectionStrings>>();
        mockConnectionStrings.Value.Returns(new ConnectionStrings
        {
            DefaultConnection = "Server=localhost;Database=nrt_test;Port=5432;User Id=postgres;Password=admin;",
            AzureConnection = "FakeConnectionString"
        });

        var options = new DbContextOptionsBuilder<AppDbContext>()
                .UseNpgsql()
                .Options;

        _context = new AppDbContext(options, mockConnectionStrings);

        _context.Database.EnsureDeleted();
        _context.Database.EnsureCreated();

        SeedData();

        var mockPaginationOptions = Substitute.For<IOptions<PaginationOptions>>();
        mockPaginationOptions.Value.Returns(new PaginationOptions
        {
            PageSize = 10
        });

        _categoryRepository = new CategoryRepository(_context, mockPaginationOptions);
    }

    private void SeedData()
    {
        var categories = new List<CategoryEntity>
        {
            new CategoryEntity { Id = 1, Name = "Root1", URL = "something.com/root", Visibility = true, Level = 1, ParentId = null },
            new CategoryEntity { Id = 2, Name = "Child 1", URL = "something.com/child1", Visibility = true, Level = 2, ParentId = 1 },
            new CategoryEntity { Id = 5, Name = "Grandchild 1.1", URL = "something.com/grandchild1.1", Visibility = true, Level = 3, ParentId = 2 },
            new CategoryEntity { Id = 6, Name = "Grandchild 1.2", URL = "something.com/grandchild1.2", Visibility = true, Level = 3, ParentId = 2 },
            new CategoryEntity { Id = 3, Name = "Child 2", URL = "something.com/child2", Visibility = true, Level = 2, ParentId = 1 },
            new CategoryEntity { Id = 7, Name = "Grandchild 2.1", URL = "something.com/grandchild2.1", Visibility = true, Level = 3, ParentId = 3 },
            new CategoryEntity { Id = 4, Name = "Child 3", URL = "something.com/child3", Visibility = true, Level = 2, ParentId = 1 },
            new CategoryEntity { Id = 8, Name = "Grandchild 3.1", URL = "something.com/grandchild3.1", Visibility = true, Level = 3, ParentId = 4 },
            new CategoryEntity { Id = 9, Name = "Grandchild 3.2", URL = "something.com/grandchild3.2", Visibility = true, Level = 3, ParentId = 4 },
            new CategoryEntity { Id = 10, Name = "Grandchild 3.3", URL = "something.com/grandchild3.3", Visibility = true, Level = 3, ParentId = 4 },
            new CategoryEntity { Id = 11, Name = "Root2", URL = "something.com/standalone1", Visibility = true, Level = 1, ParentId = null },
            new CategoryEntity { Id = 12, Name = "Root3", URL = "something.com/standalone2", Visibility = true, Level = 1, ParentId = null },
            new CategoryEntity { Id = 13, Name = "Child 1 of Root2", URL = "something.com/child1root2", Visibility = true, Level = 2, ParentId = 11 },
            new CategoryEntity { Id = 14, Name = "Child 2 of Root2", URL = "something.com/child2root2", Visibility = true, Level = 2, ParentId = 11 }
        };

        _context.Categories.AddRange(categories);


        _context.SaveChanges();
    }

    [Fact]
    public async Task GetCategorySubtreeAsync_ShouldReturnSubtreeOfSelectedCategory()
    {
        // Act
        var result = await _categoryRepository.GetCategorySubtreeAsync(1, CancellationToken.None);

        // Assert
        Assert.NotEmpty(result);
        Assert.Contains(result, c => c.Id == 1);  // Root1
        Assert.Contains(result, c => c.Id == 2);  // Child 1
        Assert.Contains(result, c => c.Id == 5);  // Grandchild 1.1
        Assert.Contains(result, c => c.Id == 6);  // Grandchild 1.2
        Assert.Contains(result, c => c.Id == 3);  // Child 2
        Assert.Contains(result, c => c.Id == 7);  // Grandchild 2.1
        Assert.Contains(result, c => c.Id == 4);  // Child 3
        Assert.Contains(result, c => c.Id == 8);  // Grandchild 3.1
        Assert.Contains(result, c => c.Id == 9);  // Grandchild 3.2
        Assert.Contains(result, c => c.Id == 10); // Grandchild 3.3
    }

    [Fact]
    public async Task GetCategoryWithHierarchyAsync_ShouldReturnCategoriesBottomUpToRoot()
    {
        // Act
        var result = await _categoryRepository.GetCategoryWithHierarchyAsync(5, CancellationToken.None);

        // Assert 
        Assert.NotEmpty(result);
        Assert.Equal(3, result.Count);  // Should contain 3 levels
        Assert.Equal(1, result[0].Id);  // Root1 
        Assert.Equal(2, result[1].Id);  // Child 1
        Assert.Equal(5, result[2].Id);  // Grandchild 1.1
    }
}
