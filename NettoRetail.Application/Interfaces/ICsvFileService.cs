using CsvHelper.Configuration;

namespace NettoRetail.Application.Interfaces;

public interface ICsvFileService
{
    Task<byte[]> GetEntitiesCsvData<T, TMap>(IEnumerable<T> entities, CancellationToken cancellationToken)
        where TMap : ClassMap<T>;
}