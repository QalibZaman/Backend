using Microsoft.Azure.Cosmos;
using Microsoft.EntityFrameworkCore;
using NettoRetail.DataAccess.Context;
using NettoRetail.Domain.Entities;
using NettoRetail.Shared.Interfaces.Repositories;
using NettoRetail.Shared.Specifications;

namespace NettoRetail.DataAccess.Repositories;

public class ProductDetailsRepository2 
{
    protected readonly DbSet<ProductDetailsEntity> _dbSet;
    protected readonly CosmosDbContext _dbContext;

    public ProductDetailsRepository2(CosmosDbContext dbContext)
    {
        _dbContext = dbContext;
        _dbSet = _dbContext.Set<ProductDetailsEntity>();
    }

    public async Task<ProductDetailsEntity> AddAsync(ProductDetailsEntity entity, CancellationToken cancellationToken)
    {
        await _dbSet.AddAsync(entity, cancellationToken);
        await _dbContext.SaveChangesAsync(cancellationToken);
        return entity;
    }

    public async Task DeleteAsync(ProductDetailsEntity entity, CancellationToken cancellationToken)
    {
        _dbSet.Remove(entity);
        await _dbContext.SaveChangesAsync(cancellationToken);
    }

    public async Task<ProductDetailsEntity?> GetAsync(Specification<ProductDetailsEntity> specification, CancellationToken cancellationToken)
    {
        var response = await _dbSet.Where(specification.Criteria)
            .FirstOrDefaultAsync(cancellationToken);

        return response;
    }

    public async Task<IEnumerable<ProductDetailsEntity>> GetManyBySpecAsync(Specification<ProductDetailsEntity> specification, CancellationToken cancellationToken)
    {
        return await _dbSet.Where(specification.Criteria).ToListAsync(cancellationToken);
    }

    public async Task DeleteManyBySpecAsync(Specification<ProductDetailsEntity> specification, CancellationToken cancellationToken)
    {
        var entitiesToDelete = await _dbSet.Where(specification.Criteria).ToListAsync();
        _dbSet.RemoveRange(entitiesToDelete);
        
        await _dbContext.SaveChangesAsync(cancellationToken);
    }

    public async Task UpdateAsync(ProductDetailsEntity entity, CancellationToken cancellationToken)
    {
        _dbSet.Update(entity);
        await _dbContext.SaveChangesAsync(cancellationToken);
    }
}

public class ProductDetailsRepository : IProductDetailsRepository
{
    private readonly Container _container;

    public ProductDetailsRepository(CosmosClient cosmosClient)
    {
        _container = cosmosClient.GetContainer("NettoRetail.CosmosDb", "ProductDetailsContainer");
    }

    public async Task<ProductDetailsEntity?> GetByIdAsync(string id, string partitionKey, CancellationToken cancellationToken = default)
    {
        try
        {
            ItemResponse<ProductDetailsEntity> response = await _container.ReadItemAsync<ProductDetailsEntity>(
                id,
                new PartitionKey(partitionKey),
                cancellationToken: cancellationToken);
            return response.Resource;
        }
        catch (CosmosException ex) when (ex.StatusCode == System.Net.HttpStatusCode.NotFound)
        {
            return null;
        }
    }

    public async Task AddAsync(ProductDetailsEntity entity, CancellationToken cancellationToken = default)
    {
        await _container.CreateItemAsync(
            entity,
            new PartitionKey(entity.InventoryItemId.ToString()),
            cancellationToken: cancellationToken);
    }

    public async Task UpsertAsync(ProductDetailsEntity entity, CancellationToken cancellationToken = default)
    {
        await _container.UpsertItemAsync(
            entity,
            new PartitionKey(entity.InventoryItemId.ToString()),
            cancellationToken: cancellationToken);
    }

    public async Task DeleteAsync(string id, string partitionKey, CancellationToken cancellationToken = default)
    {
        await _container.DeleteItemAsync<ProductDetailsEntity>(
            id,
            new PartitionKey(partitionKey),
            cancellationToken: cancellationToken);
    }

    public async Task<IEnumerable<ProductDetailsEntity>> GetAllByPartitionKeyAsync(string partitionKey, CancellationToken cancellationToken = default)
    {
        var queryDefinition = new QueryDefinition("SELECT * FROM c WHERE c.partitionKeyProperty = @partitionKey")
            .WithParameter("@partitionKey", partitionKey);

        var query = _container.GetItemQueryIterator<ProductDetailsEntity>(queryDefinition);

        var results = new List<ProductDetailsEntity>();

        while (query.HasMoreResults && !cancellationToken.IsCancellationRequested)
        {
            var response = await query.ReadNextAsync(cancellationToken);
            results.AddRange(response);
        }

        return results;
    }
}