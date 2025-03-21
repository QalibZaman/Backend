using System.Globalization;
using System.Text;
using CsvHelper;
using Mapster;
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
}