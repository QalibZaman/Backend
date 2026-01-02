using System.Globalization;
using System.Text;
using CsvHelper;
using CsvHelper.Configuration;
using Mapster;
using Microsoft.AspNetCore.Http;
using NettoRetail.Application.Exceptions;
using NettoRetail.Application.Interfaces;
using NettoRetail.Application.Models.CsvFileModel;

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
        var config = new CsvConfiguration(CultureInfo.InvariantCulture)
        {
            Delimiter = ";"
        };

        using var streamReader = new StreamReader(csvItemsFile.OpenReadStream(), Encoding.UTF8);
        using var csvReader = new CsvReader(streamReader, config);

        var entitiesData = csvReader.GetRecordsAsync<T>();

        await foreach (var entityData in entitiesData)
        {
            yield return entityData;
        }
    }

    public async Task<List<InventoryItemCsvModel>> ReadCsvItemsAsync(
        IFormFile csvFile, CancellationToken cancellationToken)
    {
        if (csvFile == null)
            throw new BadRequestException("CSV file cannot be null.");

        if (!csvFile.FileName.EndsWith(".csv", StringComparison.OrdinalIgnoreCase))
            throw new BadRequestException("The uploaded file must be a CSV file type.");

        var inventoryItemsData = ConvertEntitiesCsvFileToEntityData<InventoryItemCsvModel>(csvFile);

        var list = new List<InventoryItemCsvModel>();
        await foreach (var item in inventoryItemsData.WithCancellation(cancellationToken))
        {
            list.Add(item);
        }

        if (list.Count == 0)
            throw new BadRequestException("The CSV file is empty. Please provide at least one row of data.");

        return list;
    }

}