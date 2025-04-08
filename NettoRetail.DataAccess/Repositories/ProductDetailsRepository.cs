using Microsoft.AspNetCore.Http.HttpResults;
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

    public async Task ExecuteUpdatingTransactionalBatchAsync(
            string partitionKey,
            ICollection<ProductDetailsEntity> entitiesToUpsert,
            ICollection<string> idsToDelete,
            CancellationToken cancellationToken = default)
    {
        var batch = _container.CreateTransactionalBatch(new PartitionKey(partitionKey));

        foreach (var entity in entitiesToUpsert)
        {
            batch.UpsertItem(entity);
        }

        foreach (var id in idsToDelete)
        {
            batch.DeleteItem(id);
        }

        var response = await batch.ExecuteAsync(cancellationToken);

        if (!response.IsSuccessStatusCode)
        {
            throw new Exception($"Transaction failed with status code {response.StatusCode}");
        }
    }

    public async Task<IEnumerable<ProductDetailsEntity>> GetAllByPartitionKeyAsync(string partitionKey, CancellationToken cancellationToken = default)
    {
        try
        {
            var queryDefinition = new QueryDefinition("SELECT * FROM c WHERE c.inventoryItemId = @partitionKey")
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
