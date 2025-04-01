using Microsoft.Azure.Cosmos;
using Microsoft.Extensions.Options;
using NettoRetail.DataAccess.Options;
using NettoRetail.Domain.Entities;
using NettoRetail.Shared.Interfaces.Repositories;

namespace NettoRetail.DataAccess.Repositories;

public class ProductDetailsRepository : IProductDetailsRepository
{
    private readonly Container _container;
    private readonly CosmosClient _client;
    public ProductDetailsRepository(CosmosClient cosmosClient, IOptions<CosmosOptions> options)
    {
        _client = cosmosClient;

        _container = _client.GetContainer(options.Value.DatabaseName, options.Value.ContainerName);
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

    public async Task UpsertAsync(ProductDetailsEntity entity, CancellationToken cancellationToken = default)
    {
        var partitionKey = new PartitionKey(entity.InventoryItemId.ToString());
        await _container.UpsertItemAsync(entity, partitionKey, cancellationToken: cancellationToken);
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
        try
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
        catch (CosmosException ex) when (ex.StatusCode == System.Net.HttpStatusCode.NotFound)
        {
            return new List<ProductDetailsEntity>();
        }
    }
}