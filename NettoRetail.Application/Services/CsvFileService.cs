using System.Globalization;
using System.Text;
using CsvHelper;
using Mapster;
using Microsoft.AspNetCore.Http;
using NettoRetail.Application.Interfaces;

namespace NettoRetail.Application.Services;

public class CsvFileService : ICsvFileService
{
    public async Task<byte[]> GetEntitiesCsvData<T, TEntity>(IEnumerable<TEntity> entities, CancellationToken cancellationToken)
    {
        var mappedEntities = entities.Adapt<IEnumerable<T>>();

        using var memoryStream = new MemoryStream();
        await using var streamWriter = new StreamWriter(memoryStream, Encoding.UTF8, leaveOpen: true);
        await using (var csvWriter = new CsvWriter(streamWriter, CultureInfo.InvariantCulture))
        {
            csvWriter.WriteHeader<T>();
            await csvWriter.NextRecordAsync();
            await csvWriter.WriteRecordsAsync(mappedEntities, cancellationToken);
        }

        return memoryStream.ToArray();
    }

    public async IAsyncEnumerable<T> ConvertEntitiesCsvFileToEntityData<T>(IFormFile csvItemsFile)
    {
        using var streamReader = new StreamReader(csvItemsFile.OpenReadStream(), Encoding.UTF8);
        using var csvReader = new CsvReader(streamReader, CultureInfo.InvariantCulture);

        var entitiesData = csvReader.GetRecordsAsync<T>();

        await foreach (var entityData in entitiesData)
        {
            yield return entityData;
        }
    }
}