using System.Globalization;
using System.Text;
using CsvHelper;
using CsvHelper.Configuration;
using NettoRetail.Application.Interfaces;

namespace NettoRetail.Application.Services;

public class CsvFileService : ICsvFileService
{
    public async Task<byte[]> GetEntitiesCsvData<T, TMap>(IEnumerable<T> entities, CancellationToken cancellationToken)
        where TMap : ClassMap<T>
    {
        using var memoryStream = new MemoryStream();
        await using var streamWriter = new StreamWriter(memoryStream, Encoding.UTF8, leaveOpen: true);
        await using (var csvWriter = new CsvWriter(streamWriter, CultureInfo.InvariantCulture))
        {
            csvWriter.Context.RegisterClassMap<TMap>();
            csvWriter.WriteHeader<T>();

            await csvWriter.NextRecordAsync();
            await csvWriter.WriteRecordsAsync(entities, cancellationToken);
        }

        var entitiesCsvData = memoryStream.ToArray();

        return entitiesCsvData;
    }
}